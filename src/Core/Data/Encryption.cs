using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Gibraltar.Monitor;
using Loupe.Extensibility.Data;

namespace Gibraltar.Data
{
    /// <summary>
    /// Performs basic hybrid encryption.
    /// </summary>
    public class Encryption
    {
        private static string LogCategory = "Loupe.Encryption";
        private const int RijndaelBlockSize = 16; //in bytes
        private const int RijndaelKeySize = 16; //in bytes

        private readonly string m_KeyContainerName;
        private readonly byte[] m_PublicKey;
        private readonly string m_ExternalPublicKey;
        private readonly bool m_EncryptOnly;
        private readonly bool m_MachineStore;
        private bool m_TriedFixKeyPermissions;

        /// <summary>
        /// Create a new encryption object for use with the specified key container name
        /// </summary>
        /// <param name="keyContainerName"></param>
        /// <param name="machineStore">Indicates if the machine key store or user store should be used.</param>
        public Encryption(string keyContainerName, bool machineStore)
        {
            if (string.IsNullOrEmpty(keyContainerName))
                throw new ArgumentNullException(nameof(keyContainerName));

            m_KeyContainerName = keyContainerName;
            m_MachineStore = machineStore;

            //we need to get our local key and then its public key for encryption.  This will also make the key if it doesn't exist.
            using (RSACryptoServiceProvider rsaCryptoServiceProvider = GetLocalKey(true))
            {
                m_PublicKey = rsaCryptoServiceProvider.ExportCspBlob(false); //we only use this for encryption
                m_ExternalPublicKey = Convert.ToBase64String(m_PublicKey);

                if (string.IsNullOrEmpty(m_ExternalPublicKey))
                {
                    throw new InvalidOperationException("Unable to get public key for key container, encryption and decryption are not possible.");
                }
            }
        }

        /// <summary>
        /// Create a new encryption object for use with the specified key container name and public key.  For encryption only.
        /// </summary>
        /// <param name="publicKey"></param>
        public Encryption(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentNullException(nameof(publicKey));

            m_ExternalPublicKey = publicKey;
            m_PublicKey = Convert.FromBase64String(publicKey);
            m_EncryptOnly = true;
        }

        /// <summary>
        /// Decrypt the provided data and return a byte array.
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] encryptedData)
        {
            if (m_EncryptOnly)
                throw new InvalidOperationException("The encryption object is configured for encryption only to an external private key");

            byte[] decryptedData = PerformDecrypt(encryptedData);
            return decryptedData;
        }

        /// <summary>
        /// Decrypt the provided string and return a byte array.
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public byte[] Decrypt(string encryptedString)
        {
            byte[] encryptedData = Encoding.Unicode.GetBytes(encryptedString);

            return Decrypt(encryptedData);
        }


        /// <summary>
        /// Decrypt the provided data and return a string.
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public string DecryptString(byte[] encryptedData)
        {
            if (m_EncryptOnly)
                throw new InvalidOperationException("The encryption object is configured for encryption only to an external private key");

            byte[] decryptedData = PerformDecrypt(encryptedData);

            //now convert that from byte array to string.
            return Encoding.Unicode.GetString(decryptedData);
        }

        /// <summary>
        /// Encrypt the provided string (serialized as UTF8)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Encrypt(string data)
        {
            byte[] encodedData = Encoding.Unicode.GetBytes(data);
            return PerformEncrypt(encodedData, m_PublicKey);
        }

        /// <summary>
        /// Encrypt the provided binary data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] data)
        {
            return PerformEncrypt(data, m_PublicKey);
        }

        /// <summary>
        /// The public key used to encrypt credentials for this collection.
        /// </summary>
        public string PublicKey
        {
            get { return m_ExternalPublicKey; }
        }

        /// <summary>
        /// Clears the existing key
        /// </summary>
        public void ClearKey()
        {
            var cp = new CspParameters();
            cp.KeyContainerName = m_KeyContainerName;
            cp.Flags = CspProviderFlags.UseMachineKeyStore;
            using (var rsa = new RSACryptoServiceProvider(cp))
            {
                rsa.PersistKeyInCsp = false;
                rsa.Clear();
            }
        }

        /// <summary>
        /// Export the encryption key for this encryption scope into the specified file, overwriting it if necessary.
        /// </summary>
        /// <param name="fileNamePath"></param>
        public void ExportKey(string fileNamePath)
        {
            if (string.IsNullOrEmpty(fileNamePath))
                throw new ArgumentNullException(nameof(fileNamePath));

            File.WriteAllBytes(fileNamePath, ExportKeyToArray());
        }

        /// <summary>
        /// Export the encryption key for this encryption scope to a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ExportKeyToArray()
        {
            using (var csp = GetLocalKey())
            {
                if (csp != null)
                {
                    var keyBlob = csp.ExportCspBlob(true);
                    return keyBlob;
                }
                else
                {
                    return new byte[] { };
                }
            }
        }

        /// <summary>
        /// Import a new encryption key for this encryption scope from the specified file
        /// </summary>
        /// <param name="fileNamePath"></param>
        public void ImportKey(string fileNamePath)
        {
            if (string.IsNullOrEmpty(fileNamePath))
                throw new ArgumentNullException(nameof(fileNamePath));

            byte[] keyBlob = File.ReadAllBytes(fileNamePath);

            if (keyBlob.Length == 0)
                throw new InvalidOperationException("No key data found in the file");

            ImportKey(keyBlob);
        }

        /// <summary>
        /// Import a new encryption key for this encryption scope from the specified byte array.
        /// </summary>
        /// <param name="keyBlob"></param>
        public void ImportKey(byte[] keyBlob)
        {
            using (var csp = GetLocalKey())
            {
                if (csp != null)
                {
                    csp.ImportCspBlob(keyBlob);
                }
            }
        }

        /// <summary>
        /// Get an RSA CSP with the local encryption key loaded.  (Must be disposed by caller when done.)
        /// </summary>
        /// <returns></returns>
        private RSACryptoServiceProvider GetLocalKey(bool logDetails = false)
        {
            RSACryptoServiceProvider rsaLocalKey = null;
            CspKeyContainerInfo containerInfo = null;
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = m_KeyContainerName;

            // Check for an existing key, and report its properties.  Don't generate a new one yet
            cspParams.Flags = m_MachineStore ? CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey
                                  : CspProviderFlags.UseExistingKey;
            try
            {
                rsaLocalKey = new RSACryptoServiceProvider(cspParams);
                containerInfo = rsaLocalKey.CspKeyContainerInfo;
                int keySize = rsaLocalKey.KeySize;
                bool publicOnly = rsaLocalKey.PublicOnly;
                bool invalid = false;
                if (containerInfo.Accessible)
                {
                    bool exportable = containerInfo.Exportable;
                    bool hardwareDevice = containerInfo.HardwareDevice;
                    bool protectedKey = containerInfo.Protected;
                    bool removable = containerInfo.Removable;
                    bool randomlyGenerated = containerInfo.RandomlyGenerated;

                    if (logDetails)
                        Log.Write(LogMessageSeverity.Verbose, LogCategory, "Cryptography key found in container " + m_KeyContainerName, "A {0}-bit {1} key was found with accessible info.\r\n" +
                                  "{2}, {3}, {4}, {5}, {6} key found.\r\n", keySize, (publicOnly ? "public" : "private"),
                                  (exportable ? "Exportable" : "Non-exportable"), (randomlyGenerated ? "randomly-generated" : "non-random"),
                                  (removable ? "removable" : "non-removable"), (protectedKey ? "protected" : "unencrypted"),
                                  (hardwareDevice ? "hardware device" : "internal"));

                    if (publicOnly || protectedKey || hardwareDevice) // Don't check exportable.
                        invalid = true;

                    if (keySize != 1024 && keySize != 2048 && keySize != 1536)
                        invalid = true;
                }
                else
                {
                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Cryptography key found in container " + m_KeyContainerName, "A {0}-bit {1} key was found with inaccessible info.\r\n",
                              keySize, (publicOnly ? "public" : "private"));
                    invalid = true;
                }

                if (invalid)
                {
                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Improper local cryptography key, generating new one", "Unique key store name: {0}", m_KeyContainerName);
                    rsaLocalKey.PersistKeyInCsp = false;
                    rsaLocalKey.Clear();
                    ((IDisposable)rsaLocalKey).Dispose();
                    rsaLocalKey = null;
                    containerInfo = null;
                }
            }
            catch (Exception ex)
            {
                if (rsaLocalKey == null)
                {
                    if (m_MachineStore == false &&
                        ex.Message.StartsWith("Key not valid for use in specified state", StringComparison.OrdinalIgnoreCase))
                    {
                        // This message (with no key loaded) means whatever key is in the user's key store can't be used by them.
                        Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, LogCategory,
                                  "Local cryptography key found is broken and unusable, attempting to replace",
                                  "Unique key store name: {0}", m_KeyContainerName);
                        // We have to do an end-run around the API to try to delete the bad key file.
                        RemoveUserBadKeyFile(m_KeyContainerName);
                    }
                    else
                    {
                        Log.Write(LogMessageSeverity.Verbose, LogWriteMode.Queued, ex, LogCategory,
                                  "Local cryptography key not found, generating one", "Unique key store name: {0}",
                                  m_KeyContainerName);
                    }
                }
                else
                {
                    Log.Write(LogMessageSeverity.Verbose, LogWriteMode.Queued, ex, LogCategory,
                              "Error examining cryptography key, generating new one", "Unique key store name: {0}",
                              m_KeyContainerName);
                    rsaLocalKey.PersistKeyInCsp = false;
                    rsaLocalKey.Clear();
                    ((IDisposable)rsaLocalKey).Dispose();
                    rsaLocalKey = null;
                    containerInfo = null;
                }
            }

            if (rsaLocalKey == null)
            {
                // Didn't already exist, make a new one.
                cspParams.CryptoKeySecurity = null;

                // Don't let this export the private key (public okay).
                cspParams.Flags = m_MachineStore ? CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseArchivableKey
                                      : CspProviderFlags.UseArchivableKey;

                rsaLocalKey = new RSACryptoServiceProvider(1024, cspParams);
                containerInfo = rsaLocalKey.CspKeyContainerInfo;

                m_TriedFixKeyPermissions = true; // We're about to do it, so disable further fixing attempts.
                FixKeyPermissions(containerInfo.UniqueKeyContainerName, m_KeyContainerName, true, logDetails); // Fix permissions on the new key.
            }
            else if (m_TriedFixKeyPermissions == false) // Already existed, only try to fix it once...
            {
                m_TriedFixKeyPermissions = true; // We'll try once, then assume it's done or will keep failing.
                FixKeyPermissions(containerInfo.UniqueKeyContainerName, m_KeyContainerName, true, logDetails); // Try to fix once for existing key.
            }

            return rsaLocalKey;
        }


        private static void FixKeyPermissions(string uniqueKeyName, string label, bool persisting, bool logDetails = false)
        {
            if (string.IsNullOrEmpty(uniqueKeyName))
                return;

            string labelString;
            if (string.IsNullOrEmpty(label))
                labelString = string.Empty;
            else
                labelString = string.Format(" for {0} key", label);

            try
            {
                string keyStorePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                keyStorePath = Path.Combine(keyStorePath, "Microsoft");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                keyStorePath = Path.Combine(keyStorePath, "Crypto");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                keyStorePath = Path.Combine(keyStorePath, "RSA");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                keyStorePath = Path.Combine(keyStorePath, "MachineKeys");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                string[] keyFiles = Directory.GetFiles(keyStorePath, uniqueKeyName);

                if (keyFiles.Length != 1)
                    return;

                if (persisting && logDetails)
                {
                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Checking key permissions" + labelString, null);
                }

                const FileSystemRights userRights = FileSystemRights.Modify | FileSystemRights.Delete |
                                                    FileSystemRights.Read | FileSystemRights.Write;
                SecurityIdentifier usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null); // Hope null works here.
                SecurityIdentifier adminsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                SecurityIdentifier ownerSid = new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null);
                FileSystemAccessRule usersRule = new FileSystemAccessRule(usersSid, userRights, AccessControlType.Allow);
                FileSystemAccessRule adminsRule = new FileSystemAccessRule(adminsSid, userRights, AccessControlType.Allow);
                FileSystemAccessRule ownerRule = new FileSystemAccessRule(ownerSid, FileSystemRights.FullControl, AccessControlType.Allow);
                foreach (string keyFile in keyFiles)
                {
                    try
                    {
                        FileSecurity fileSecurity;
                        AuthorizationRuleCollection rules;
                        if (persisting == false)
                        {
                            // Vault sets inheriting permissions on the folder which break the default owner permissions.
                            // If persisting is false, we must have just created a temp key, so we should be the owner.
                            // Thus we have the right to read and change permissions, but that could be all we have!
                            // So we need to get and fix the owner permissions without checking if the file exists.
                            fileSecurity = File.GetAccessControl(keyFile);
                            rules = fileSecurity.GetAccessRules(true, false, typeof(SecurityIdentifier));
                            fileSecurity.AddAccessRule(ownerRule);

                            File.SetAccessControl(keyFile, fileSecurity);
                        } // TODO: else if poison... allow read-for-all deny write for all?
                        else if (File.Exists(keyFile)) // It won't "exist" if we can't write to it, so we probably couldn't do this.
                        {
                            bool needed = true;
                            bool adminsNeeded = true;
                            fileSecurity = File.GetAccessControl(keyFile);
                            rules = fileSecurity.GetAccessRules(true, false, typeof(SecurityIdentifier));
                            foreach (AuthorizationRule rule in rules)
                            {
                                FileSystemAccessRule fileRule = rule as FileSystemAccessRule;
                                if (fileRule != null && fileRule.AccessControlType == AccessControlType.Allow)
                                {
                                    if ((fileRule.FileSystemRights & userRights) == userRights) // Has at least Modify rights...
                                    {
                                        if (fileRule.IdentityReference == usersSid)
                                            needed = false;
                                        else if (fileRule.IdentityReference == adminsSid)
                                            adminsNeeded = false;
                                    }
                                }
                            }

                            if (needed || adminsNeeded)
                            {
                                fileSecurity.AddAccessRule(ownerRule);
                                fileSecurity.AddAccessRule(usersRule);
                                fileSecurity.AddAccessRule(adminsRule);
                                File.SetAccessControl(keyFile, fileSecurity);
                                if (logDetails)
                                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Key permissions successfully set" + labelString, null);
                            }
                            else
                            {
                                if (logDetails)
                                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Key permissions already set" + labelString, null);
                            }
                        }
                        else
                        {
                            Log.Write(LogMessageSeverity.Information, LogCategory, "Key permissions could not be checked" + labelString, null);
                        }
                    }
                    catch
                    {
                        if (persisting)
                            Log.Write(LogMessageSeverity.Warning, LogCategory, "Key permissions could not be set" + labelString, null);
                    }
                }
            }
            catch
            {
                Log.Write(LogMessageSeverity.Information, LogCategory, "Key permissions could not be found" + labelString, null);
            }
        }

        private static void RemoveUserBadKeyFile(string keyContainerName)
        {
            if (string.IsNullOrEmpty(keyContainerName))
                return;

            string filePattern = null;
            if (keyContainerName == "Gibraltar_Credentials_User")
                filePattern = "7233e1ab2a599ff9a1d76ffcf1b7e93a_*";
            else if (keyContainerName == "Gibraltar_Test_Credentials_User")
                filePattern = "05a0c0ce7d6b6b6663f15e5fd2a1b366_*";
            else
                return;

            string targetKeyFile = null;
            try
            {
                string keyStorePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // For roaming user.
                keyStorePath = Path.Combine(keyStorePath, "Microsoft");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                keyStorePath = Path.Combine(keyStorePath, "Crypto");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                keyStorePath = Path.Combine(keyStorePath, "RSA");
                if (Directory.Exists(keyStorePath) == false)
                    return;

                string[] userKeyStores = Directory.GetDirectories(keyStorePath, "S-*", SearchOption.TopDirectoryOnly);
                int folderCount = userKeyStores.Length;
                Log.Write(LogMessageSeverity.Verbose, LogCategory, null, "Found {0} user key store folder{1}{2}.", folderCount,
                          folderCount == 1 ? string.Empty : "s", folderCount > 0 ? ".  Searching.." : string.Empty);

                foreach (string userKeyStore in userKeyStores)
                {
                    string[] keyFiles = Directory.GetFiles(userKeyStore, filePattern);
                    if (keyFiles.Length < 1)
                        continue; // None found there.

                    if (string.IsNullOrEmpty(targetKeyFile) == false)
                    {
                        Log.Write(LogMessageSeverity.Verbose, LogCategory, null,
                                  "More than one user key store folder has container files matching:\r\n'{0}'\r\n" +
                                  "Unable to proceed with deletion", filePattern);
                        targetKeyFile = null;
                        break;
                    }

                    if (keyFiles.Length > 1)
                    {
                        Log.Write(LogMessageSeverity.Verbose, LogCategory, null,
                                  "Multiple key container files matching '{0}'\r\nfound in: '{1}'\r\n" +
                                  "Unable to proceed with deletion.", filePattern, userKeyStore);
                        targetKeyFile = null;
                        break;
                    }

                    // There is exactly one in that folder.  Make sure there are no other matches in other folders.
                    targetKeyFile = keyFiles[0];
                }

                if (string.IsNullOrEmpty(targetKeyFile) == false)
                {
                    Log.Write(LogMessageSeverity.Information, LogCategory, null, "Deleting bad key container: {0}", targetKeyFile);
                    File.Delete(targetKeyFile);
                }
            }
            catch (Exception ex)
            {
                Log.Write(string.IsNullOrEmpty(targetKeyFile) ? LogMessageSeverity.Information : LogMessageSeverity.Warning,
                          LogCategory, null, "Unable to delete key bad container file for {0}", keyContainerName);
                GC.KeepAlive(ex);
            }
        }

        private byte[] PerformEncrypt(byte[] data, byte[] publicKey)
        {
            byte[] cipherText;
            using (RijndaelManaged rijndaelEncrypt = new RijndaelManaged())
            {
                rijndaelEncrypt.BlockSize = RijndaelBlockSize * 8; // AES uses fixed block size of 128 (smallest of Rijndael). 16 bytes.
                rijndaelEncrypt.KeySize = RijndaelKeySize * 8; // AES allows 128- 192- or 256-bit keys (supported by RijndaelManaged). 16 bytes.
                rijndaelEncrypt.Mode = CipherMode.CFB;
                rijndaelEncrypt.Padding = PaddingMode.None;
                rijndaelEncrypt.FeedbackSize = 8; // 8-bit CFB.
                rijndaelEncrypt.GenerateKey();
                rijndaelEncrypt.GenerateIV();
                byte[] key = rijndaelEncrypt.Key;
                byte[] iv = rijndaelEncrypt.IV;

                byte[] rsaPayload = new byte[iv.Length + key.Length];

                // Put the fixed-size 16-byte IV first.
                Array.Copy(iv, rsaPayload, RijndaelBlockSize);

                // Put the key second, in case we make it bigger later.
                Array.Copy(key, 0, rsaPayload, RijndaelBlockSize, key.Length);

                byte[] encryptedKey = null;
                CspParameters cspParams = new CspParameters();
                cspParams.CryptoKeySecurity = null;
                cspParams.Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.NoPrompt;
                using (RSACryptoServiceProvider rsaEncrypt = new RSACryptoServiceProvider(cspParams))
                {
                    try
                    {
                        //FixKeyPermissions(rsaEncrypt.CspKeyContainerInfo.UniqueKeyContainerName, m_KeyContainerName, false);
                        rsaEncrypt.PersistKeyInCsp = false; // We don't want to use the key store, so make sure it doesn't try.
                        rsaEncrypt.ImportCspBlob(publicKey);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, LogCategory, "Invalid machine public key submitted.",
                                  "Failed to import as CSP Blob.\r\nPublic key string: {0}\r\n", publicKey);

                        throw;
                    }

                    encryptedKey = rsaEncrypt.Encrypt(rsaPayload, false);
                }

                byte[] prefix = BinarySerializer.SerializeValue(encryptedKey.Length);
                using (MemoryStream memoryStreamEncrypt = new MemoryStream())
                {
                    memoryStreamEncrypt.Write(prefix, 0, prefix.Length); // Copy the encoded RSA length.
                    memoryStreamEncrypt.Write(encryptedKey, 0, encryptedKey.Length); // Copy the RSA-encrypted header.

                    using (ICryptoTransform encryptTransform = rijndaelEncrypt.CreateEncryptor())
                    {
                        using (CryptoStream cryptoStreamEncrypt = new CryptoStream(memoryStreamEncrypt, encryptTransform,
                                                                                   CryptoStreamMode.Write))
                        {
                            cryptoStreamEncrypt.Write(data, 0, data.Length); // Symmetric-encrypt the payload.
                            cryptoStreamEncrypt.Close(); // Make sure this gets flushed before we extract the array.
                        }
                    }

                    cipherText = memoryStreamEncrypt.ToArray();
                }
            }
            return cipherText;
        }

        /// <summary>
        /// Decrypted the provided encrypted data payload using the existing symmetric key on this computer.
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns>Unencrypted data</returns>
        private byte[] PerformDecrypt(byte[] encryptedData)
        {
            if ((encryptedData == null) || (encryptedData.Length == 0))
                return new byte[0];

            byte[] rsaPayload;
            byte[] cipherText;
            int cipherTextLength;

            try
            {
                using (RSACryptoServiceProvider rsaDecrypt = GetLocalKey())
                {
                    rsaDecrypt.PersistKeyInCsp = true; // We DO want to keep this in the machine key store (true by default anyway).
                    byte[] encryptedKey;
                    using (MemoryStream encryptedDataStream = new MemoryStream(encryptedData))
                    {
                        int encryptedKeyLength;
                        BinarySerializer.DeserializeValue(encryptedDataStream, out encryptedKeyLength);
                        encryptedKey = new byte[encryptedKeyLength];
                        encryptedDataStream.Read(encryptedKey, 0, encryptedKeyLength);
                        cipherTextLength = encryptedData.Length - (int)encryptedDataStream.Position;
                        cipherText = new byte[cipherTextLength];
                        encryptedDataStream.Read(cipherText, 0, cipherTextLength);
                    }

                    rsaPayload = rsaDecrypt.Decrypt(encryptedKey, false);
                }
            }
            catch (OverflowException ex)
            {
                throw new CryptographicException("Unable to decrypt data due to " + ex.GetType(), ex);
            }

            byte[] iv = new byte[RijndaelBlockSize]; // Fixed Rijndael block size;
            int keyLength = rsaPayload.Length - RijndaelBlockSize;
            byte[] key = new byte[keyLength]; // Probably 16, but we could make it 24 or 32 later.

            Array.Copy(rsaPayload, iv, RijndaelBlockSize);
            Array.Copy(rsaPayload, RijndaelBlockSize, key, 0, keyLength);

            byte[] recoveredData;
            using (MemoryStream memoryStreamDecrypt = new MemoryStream())
            {
                using (RijndaelManaged rijndaelDecrypt = new RijndaelManaged())
                {
                    rijndaelDecrypt.BlockSize = RijndaelBlockSize * 8; //thats in bits
                    rijndaelDecrypt.Key = key;
                    rijndaelDecrypt.IV = iv;
                    rijndaelDecrypt.Mode = CipherMode.CFB;
                    rijndaelDecrypt.Padding = PaddingMode.None;
                    rijndaelDecrypt.FeedbackSize = 8;

                    using (ICryptoTransform decryptTransform = rijndaelDecrypt.CreateDecryptor())
                    {
                        using (CryptoStream cryptoStreamDecrypt = new CryptoStream(memoryStreamDecrypt, decryptTransform,
                                                                                   CryptoStreamMode.Write))
                        {
                            cryptoStreamDecrypt.Write(cipherText, 0, cipherTextLength);
                            cryptoStreamDecrypt.Close(); // Make sure this gets flushed before we extract the array.
                        }
                    }
                }

                recoveredData = memoryStreamDecrypt.ToArray();
            }

            return recoveredData;
        }

    }
}
