
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// The set of defined extent typecodes designating the class needed to handle an extent starting with that code.
    /// </summary>
    public enum ExtentTypeCode : long
    {
        /// <summary>Dummy value for completeness.</summary>
        None = 0,

        // File typecodes are modeled after PNG, except "GLF" (etc) substituted for "PNG".  This format is designed
        // to help prevent our binary-format files from being handled in screwy ways by other software.

        /// <summary>The typecode used by the original (legacy) GLF file format (prior to Gibraltar 2.2).</summary>
        GibraltarLegacyGlf = 0x79474c460d0a1a0a, // 'yGLF\r\n^Z\n'   
        /// <summary>A session Fragment File which does not contain a Header extent but contains packet data.</summary>
        GibraltarFragmentFile = 0x79474c660c0a1a0a, // 'yGLf\r\n^Z\n'
        /// <summary>A session Header File which does not contain packet data.</summary>
        GibraltarHeaderFile = 0x79474c680c0a1a0a, // 'yGLh\r\n^Z\n'
        /// <summary>A merged Session File containing both a Header extent and an extent encapsulating packet data.</summary>
        GibraltarSessionFile = 0x79474c730c0a1a0a, // 'yGLs\r\n^Z\n'

        // Internal extent typecodes will not be seen at the start of a file (an ExtentPreamble with a file typecode goes first),
        // so they have more freedom in the format.  Gibraltar Logging typecodes should start with 'yglf' = 0x79676c66 or
        // at least 'ygl'.  If this Common assembly is used in other product groups they should use a new prefix.
        // Using readable ASCII letters is recommended.

        /// <summary>A Session Header extent containing a full session header.</summary>
        GibraltarSessionHeader = 0x79676c6653734864, // 'yglfSsHd'

        /// <summary>A Fragment Collection extent collecting one or more Fragment Record extents.</summary>
        GibraltarFragmentCollection = 0x79676c664672436c, // 'yglfFrCl'

        /// <summary>A Fragment Record extent containing information identifying a fragment file of this session.</summary>
        GibraltarFragmentRecord = 0x79677c7746725263, // 'yglfFrRc'

        /// <summary>An Attachment Collection extent collecting one or more Attachment Record extents.</summary>
        GibraltarAttachmentCollection = 0x79676c664174436c, // 'yglfAtCl'

        /// <summary>An Attachment Record extent containing information identifying a file attached to this session.</summary>
        GibraltarAttachmentRecord = 0x79676c6641745263, // 'yglfAtRc'

        /// <summary>A Property Collection extent collecting one or more Property Record extents.</summary>
        GibraltarPropertyCollection = 0x79676c665072436c, // 'yglfPrCl'

        /// <summary>A Property Record extent containing one (or more?) name/value property pairs.</summary>
        GibraltarPropertyRecord = 0x79676c6650725263, // 'yglfPrRc'



        /// <summary>A Fragment Header extent containing information identifying this Fragment File.</summary>
        GibraltarFragmentHeader = 0x79676c6646724864, // 'yglfFrHd'

        /// <summary>A Packet Stream extent containing Gibraltar log data packets.</summary>
        GibraltarPacketStream = 0x79676c66506b5374, // 'yglfPkSt'

        /// <summary>A Key Collection extent containing one or more Key Description extents.</summary>
        GibraltarKeyCollection = 0x79676c664b79436c, // 'yglfKyCl'

        /// <summary>A Key Description extent containing an RSA-encrypted symmetric key.</summary>
        GibraltarKeyDescription = 0x79676c664b794473, // 'yglfKyDs'

        /// <summary>An Encrypted Data extent containing a Rijndael-encrypted compressed packet stream.</summary>
        GibraltarEncryptedData = 0x79676c6645435053, // 'yglfECPS'



        /// <summary>A Session Archive extent containing a gzip archive of Fragment Files and attached files.</summary>
        GibraltarSessionArchive = 0x79676c6653734172, // 'yglfSsAr'

        /// <summary>An Encrypted Archive extent containing a Rijndael-encrypted gzip archive of Fragment Files, etc.</summary>
        GibraltarEncryptedArchive = 0x79676c6653734172, // 'yglfSsAr'

        // |41 |42 |43 |44 |45 |46 |47 |48 |49 |4a |4b |4c |4d |4e |4f |50 |51 |52 |53 |54 |55 |56 |57 |58 |59 |5a |
        // |A a|B b|C c|D d|E e|F f|G g|H h|I i|J j|K k|L l|M m|N n|O o|P p|Q q|R r|S s|T t|U u|V v|W w|X x|Y y|Z z|
        // | 61| 62| 63| 64| 65| 66| 67| 68| 69| 6a| 6b| 6c| 6d| 6e| 6f| 70| 71| 72| 73| 74| 75| 76| 77| 78| 79| 7a|
    }
}
