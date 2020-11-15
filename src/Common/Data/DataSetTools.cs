
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Data;

#endregion File Header

namespace Gibraltar.Data
{
    /// <summary>
    /// Extensions to ADO.NET for working with data sets and data views
    /// </summary>
    public static class DataSetTools
    {
        /// <summary>
        /// Compares two column values to see if they are equal, handling DBNull correctly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool ColumnEqual(object a, object b)
        {
            // Compares two values to see if they are equal. Also compares DBNULL.Value.
            // If your DataTable contains object fields, then you must extend this
            // function to handle them in a meaningful way if you intend to group on them.

            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) //both are null
                return true;
            if (a == DBNull.Value && b == DBNull.Value) //  both are DBNull.Value
                return true;

            if ((a is string) || (b is string)) //if either are string we should be able to use string handling; both SHOULD be the same type.  OR because null won't compare right
            {
                string leftString = (a == DBNull.Value) ? null : (string)a;
                string rightString = (b == DBNull.Value) ? null : (string)b;

                if (string.IsNullOrEmpty(leftString) && string.IsNullOrEmpty(rightString))
                    return true; //we treat these as a variation of null, and therefore equal.
                if (string.IsNullOrEmpty(leftString) || string.IsNullOrEmpty(rightString))
                    return false; //one is null;

                //special string comparison (now that we know neither is null or DBNull)
                return ((string)a).Equals((string)b, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                if (a == DBNull.Value || b == DBNull.Value) //  only one is DBNull.Value
                    return false;
                if (a == null || b == null) //  only one is null
                    return false;

                return (a.Equals(b));  // value type standard comparison                
            }
        }


        /// <summary>
        /// Create a new data set exclusively with the rows that match the provided filter.
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="filter"></param>
        /// <returns>A new data set that has just the rows that match the provided filter, meaning that
        /// it can't be removed by a subsequent operation. The original data set is unaffected.</returns>
        public static DataSet FilterDataSet(DataSet sourceData, string filter)
        {
            return FilterDataSet(sourceData, filter, null);
        }


        /// <summary>
        /// Create a new data set exclusively with the rows that match the provided filter.
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="filter">A data table filter clause</param>
        /// <param name="sortOrder">A data table sort clause</param>
        /// <returns>A new data set that has just the rows that match the provided filter, meaning that
        /// it can't be removed by a subsequent operation. The original data set is unaffected.</returns>
        public static DataSet FilterDataSet(DataSet sourceData, string filter, string sortOrder)
        {
            if (sourceData == null)
                throw new ArgumentNullException(nameof(sourceData));

            if (string.IsNullOrEmpty(filter))
            {
                //no exception, just return a copy of what we got.
                return sourceData.Copy();
            }

            DataSet filteredDataSet = sourceData.Copy();
            filteredDataSet.Clear(); //get rid of all of the rows

            //I haven't used reflection to check that the select method is the same with no argument specified and with a null value, so we separate them out.
            if (string.IsNullOrEmpty(sortOrder))
            {
                filteredDataSet.Merge(sourceData.Tables[0].Select(filter));                
            }
            else
            {
                filteredDataSet.Merge(sourceData.Tables[0].Select(filter, sortOrder));                
            }

            return filteredDataSet;
        }

        /// <summary>
        /// Generates a data table with two columns (Value and Caption) of the distinct values in the provided data table
        /// </summary>
        /// <param name="tableName">The name of the data table generated and returned</param>
        /// <param name="sourceTable">The source data table to inspect for distinct values</param>
        /// <param name="valueColumnName">The column to check for distinct values</param>
        /// <param name="captionColumnName">The column with the display value for the distinct value.</param>
        /// <param name="includeAll">If true, the distinct values will include a row with null for value and (All) for caption</param>
        /// <param name="includeNull">If true, null values will be included as a distinct value</param>
        /// <returns></returns>
        public static DataTable SelectDistinctValues(string tableName, DataTable sourceTable, string valueColumnName, string captionColumnName, bool includeAll, bool includeNull = true)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (sourceTable == null)
                throw new ArgumentNullException(nameof(sourceTable));

            if (string.IsNullOrEmpty(valueColumnName))
                throw new ArgumentNullException(nameof(valueColumnName));

            if (string.IsNullOrEmpty(captionColumnName))
                throw new ArgumentNullException(nameof(captionColumnName));

            DataTable distinctValuesTable = new DataTable(tableName);
            DataColumn valueColumn = sourceTable.Columns[valueColumnName];
            DataColumn captionColumn = sourceTable.Columns[captionColumnName];

            if (valueColumn == null)
                throw new ArgumentException("No column could be found with the name " + valueColumnName, nameof(valueColumnName));
            if (captionColumn == null)
                throw new ArgumentException("No column could be found with the name " + captionColumnName, nameof(captionColumnName));

            distinctValuesTable.Columns.Add("Value", valueColumn.DataType);
            distinctValuesTable.Columns.Add("Caption", captionColumn.DataType);

            if (includeAll)
            {
                distinctValuesTable.Rows.Add(new object[] {null, "(All)"});
            }

            object lastValue = null;

            //order by the value column so we can take advantage of waiting for values to change.
            string filter = includeNull ? string.Empty : valueColumnName + " IS NOT NULL";
            foreach (DataRow curRow in sourceTable.Select(filter, valueColumnName)) //null values won't go in our list.
            {
                object currentValue = curRow[valueColumn];
                
                if (((lastValue == null) && includeNull) 
                    || !(ColumnEqual(lastValue, currentValue)))
                {
                    lastValue = currentValue;
                    object displayValue = curRow[captionColumn];

                    if (displayValue is string)
                    {
                        displayValue = string.IsNullOrEmpty((string)displayValue) ? null : displayValue;
                    }

                    if ((displayValue == null) || (displayValue == DBNull.Value))
                        displayValue = "(None)";

                    distinctValuesTable.Rows.Add(new[] { lastValue, displayValue });
                }
            }

            return distinctValuesTable;
        }

        /// <summary>
        /// GEnerates a data table with just the top n rows of the provided table, based on its default sort.
        /// </summary>
        /// <param name="sourceView">The filtered and sorted view to take the top 10 rows from</param>
        /// <param name="rows">The maximum number of rows to return</param>
        /// <param name="keyColumnName">Optional.  The name of the primary key column to eliminate duplicates.</param>
        /// <returns></returns>
        public static DataView SelectTop(DataView sourceView, int rows, string keyColumnName)
        {
            if (sourceView == null)
                throw new ArgumentNullException(nameof(sourceView));


            DataTable sourceTable = sourceView.ToTable();
            DataTable destinationTable = sourceTable.Clone(); //to get structure w/o data

            //if we're using a key filter we need to set a primary key on the destination table.
            bool useKeyFilter = false;
            if (string.IsNullOrEmpty(keyColumnName) == false)
            {
                destinationTable.PrimaryKey = new[]{ destinationTable.Columns[keyColumnName]};
                useKeyFilter = true;
            }

            for (int curRowIndex = 0; curRowIndex < sourceTable.Rows.Count; curRowIndex++)
            {
                //See if this new row is unique based on the provided key column (if provided)
                DataRow currentRow = sourceTable.Rows[curRowIndex];

                //if we aren't using a key filter or we can't find the specified key already in our set, add the row.
                if ((useKeyFilter == false) || (destinationTable.Rows.Find(currentRow[keyColumnName]) == null))
                {
                    destinationTable.ImportRow(sourceTable.Rows[curRowIndex]);
                }

                //and if we've reached our quota, we're done whether there are more rows or not.
                if (destinationTable.Rows.Count == rows)
                    break;
            }

            return new DataView(destinationTable);
        }

        /// <summary>
        /// Create a new data table by joining the two provided data tables.
        /// </summary>
        /// <param name="firstTable"></param>
        /// <param name="secondTable"></param>
        /// <param name="firstJoinColumnArray"></param>
        /// <param name="secondJoinColumnArray"></param>
        /// <returns></returns>
        public static DataTable Join(DataTable firstTable, DataTable secondTable, DataColumn[] firstJoinColumnArray, DataColumn[] secondJoinColumnArray)
        {
            if (firstTable == null)
                throw new ArgumentNullException(nameof(firstTable));

            if (secondTable == null)
                throw new ArgumentNullException(nameof(secondTable));

            if (firstJoinColumnArray == null)
                throw new ArgumentNullException(nameof(firstJoinColumnArray));

            if (secondJoinColumnArray == null)
                throw new ArgumentNullException(nameof(secondJoinColumnArray));


            //Create Empty Table
            DataTable table = new DataTable("Join");
            // Use a DataSet to leverage DataRelation
            using (DataSet ds = new DataSet())
            {
                //Add Copy of Tables
                ds.Tables.AddRange(new[] { firstTable.Copy(), secondTable.Copy() });

                //Identify Joining Columns from First
                DataColumn[] parentcolumns = new DataColumn[firstJoinColumnArray.Length];
                for (int i = 0; i < parentcolumns.Length; i++)
                {
                    parentcolumns[i] = ds.Tables[0].Columns[firstJoinColumnArray[i].ColumnName];
                }

                //Identify Joining Columns from Second
                DataColumn[] childcolumns = new DataColumn[secondJoinColumnArray.Length];
                for (int i = 0; i < childcolumns.Length; i++)
                {
                    childcolumns[i] = ds.Tables[1].Columns[secondJoinColumnArray[i].ColumnName];
                }

                //Create DataRelation
                DataRelation r = new DataRelation(string.Empty, parentcolumns, childcolumns, false);
                ds.Relations.Add(r);

                //Create Columns for JOIN table
                for (int i = 0; i < firstTable.Columns.Count; i++)
                {
                    table.Columns.Add(firstTable.Columns[i].ColumnName, firstTable.Columns[i].DataType);
                }
                for (int i = 0; i < secondTable.Columns.Count; i++)
                {
                    //Beware Duplicates
                    if (!table.Columns.Contains(secondTable.Columns[i].ColumnName))
                        table.Columns.Add(secondTable.Columns[i].ColumnName, secondTable.Columns[i].DataType);
                    else
                        table.Columns.Add(secondTable.Columns[i].ColumnName + "_Second", secondTable.Columns[i].DataType);
                }

                //Loop through First table
                table.BeginLoadData();
                foreach (DataRow firstrow in ds.Tables[0].Rows)
                {
                    //Get "joined" rows
                    DataRow[] childrows = firstrow.GetChildRows(r);
                    if (childrows != null && childrows.Length > 0)
                    {
                        object[] parentarray = firstrow.ItemArray;
                        foreach (DataRow secondrow in childrows)
                        {
                            object[] secondarray = secondrow.ItemArray;
                            object[] joinarray = new object[parentarray.Length + secondarray.Length];
                            Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                            Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                            table.LoadDataRow(joinarray, true);
                        }
                    }
                }

                table.EndLoadData();
            }
            return table;
        }

        /// <summary>
        /// Create a new table by joining together two tables on the provided column
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="firstJoinColumn"></param>
        /// <param name="secondJoinColumn"></param>
        /// <returns></returns>
        public static DataTable Join(DataTable first, DataTable second, DataColumn firstJoinColumn, DataColumn secondJoinColumn)
        {
            return Join(first, second, new[] { firstJoinColumn }, new[] { secondJoinColumn });
        }
        
        /// <summary>
        /// Create a new table by joining together two tables on the provided column
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="firstJoinColumn"></param>
        /// <param name="secondJoinColumn"></param>
        /// <returns></returns>
        public static DataTable Join(DataTable first, DataTable second, string firstJoinColumn, string secondJoinColumn)
        {
            return Join(first, second, new[] { first.Columns[firstJoinColumn] }, new[] { first.Columns[secondJoinColumn] });
        }

        /// <summary>
        /// Safely read a nullable column value, translating DBNull to null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adoColumnValue"></param>
        /// <returns></returns>
        public static T ReadNullableObject<T>(object adoColumnValue) where T: class
        {
            if (adoColumnValue == DBNull.Value)
                return null;

            return (T)adoColumnValue;
        }


        /// <summary>
        /// Safely read a nullable column value, translating DBNull to null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adoColumnValue"></param>
        /// <returns></returns>
        public static Nullable<T> ReadNullableValue<T>(object adoColumnValue) where T : struct 
        {
            if (adoColumnValue == DBNull.Value)
                return null;

            return (T)adoColumnValue;
        }

        /// <summary>
        /// Safely read a nullable column value, translating DBNull to null.
        /// </summary>
        /// <param name="adoColumnValue"></param>
        /// <returns></returns>
        public static object ReadDbNullValue(object adoColumnValue)
        {
            if (adoColumnValue == DBNull.Value)
                return null;

            return adoColumnValue;
        }
    }
}
