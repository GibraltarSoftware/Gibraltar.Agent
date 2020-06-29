#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System.Drawing;
using SourceGrid;
using SourceGrid.Cells;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    internal class GridViewerRows : DataGridRows
    {
        private DataGridColumn m_MessageColumn;

        public GridViewerRows(DataGrid grid)
            : base(grid)
        {
            MaxRowHeight = 500;
            MinRowHeight = 20; // 16-pixel image plus 2-pixel border top and bottom = 20 pixels
        }

        public DataGridColumn MessageColumn { get; set; }

        public int MaxRowHeight { get; set; }

        public int MinRowHeight { get; set; }

        public override int GetHeight(int row)
        {
            if (row == 0)
                return HeaderHeight;

            // We apparently sometimes get called when the window is going away (maybe mostly after sitting in a breakpoint?),
            // so the call to Measure() can get an Exception.  So we'll do this in a try/catch to guard against it.
            // Hopefully, this won't just push the errors elsewhere.
            int height;
            try
            {
                // Use the Message cell to drive row height

                if (m_MessageColumn == null)
                {
                    //hunt for it..
                    var gridViewer = (GridViewer)Grid;
                    m_MessageColumn = gridViewer.GetColumnForEnum(LogMessageColumn.Caption);
                }

                if (m_MessageColumn == null)
                    return HeaderHeight;

                int columnIndex = m_MessageColumn.Index;
                ICellVirtual cell = Grid.GetCell(row, columnIndex);
                CellContext context = new CellContext(Grid, new Position(row, columnIndex));
                Size measure = cell.View.Measure(context, new Size(Grid.Columns[columnIndex].Width, MaxRowHeight));
                height = measure.Height;
            }
            catch
            {
                height = 0; // Let it get set to the minimum.
            }

            if (height < MinRowHeight)
                height = MinRowHeight;

            return height;
        }
    }
}
