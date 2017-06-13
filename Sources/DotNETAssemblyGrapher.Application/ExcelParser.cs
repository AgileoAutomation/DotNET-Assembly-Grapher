using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Windows.Forms;

namespace DotNETAssemblyGrapherApplication
{
    public class ExcelParser : IFileParser
    {
        List<Row> rows;
        WorkbookPart workbookPart;

        private string TextInCell(Cell c)
        {
            string text = c.InnerText;
            var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (stringTable != null && !string.IsNullOrEmpty(text))
                return stringTable.SharedStringTable.ElementAt(int.Parse(text)).InnerText;
            else
                return null;
        }

        public List<ComponentSpecification> BuildSpecifications(string filepath)
        {
            List<ComponentSpecification> specs = new List<ComponentSpecification>();

            try
            {
                SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, false);
                workbookPart = spreadsheetDocument.WorkbookPart;
            }
            catch
            {
                MessageBox.Show("The specification file is open in a other process");
                return null;
            }
            
            SheetData sheetData = null;

            try
            {
                Sheet sheet = workbookPart.Workbook.Sheets.ChildElements.Cast<Sheet>().First(x => x.Name == "Assembly Info");
                int index = workbookPart.WorksheetParts.ToList().IndexOf(workbookPart.WorksheetParts.Last()) - workbookPart.Workbook.Sheets.ToList().IndexOf(sheet);
                sheetData = workbookPart.WorksheetParts.ElementAt(index).Worksheet.Elements<SheetData>().First();
            }
            catch
            {
                MessageBox.Show("Invalid specification file :\nCouldn't find the 'Assembly Info' worksheet");
                return null;
            }
            

            rows = new List<Row>();
            rows.AddRange(sheetData.Elements<Row>());
            
            int assembliesColumn = -1;

            try
            {
                List<Cell> headerRow = rows.First().Elements<Cell>().ToList();
                assembliesColumn = headerRow.IndexOf(headerRow.First(x => TextInCell(x) == "Assemblies"));
            }
            catch
            {
                MessageBox.Show("Invalid specification file :\nPlease respect the spreadsheet pattern, you didn't specified the assembly column");
            }

            rows.RemoveAt(0);
            rows.RemoveAll(x => !x.Elements<Cell>().Any(y => !string.IsNullOrEmpty(TextInCell(y))));

            while (rows.Count > 0)
            {
                specs.Add(BuildSpec(0, assembliesColumn));
            }

            return specs;
        }

        private ComponentSpecification BuildSpec(int currentColumn, int assembliesColumn)
        {
            WorkbookStylesPart styles = workbookPart.WorkbookStylesPart;
            Stylesheet stylesheet = styles.Stylesheet;

            List<Cell> row = rows.First().Elements<Cell>().ToList();
            Cell currentCell = row.ElementAt(currentColumn);
            string cellContent = TextInCell(currentCell);

            ComponentSpecification component = new ComponentSpecification(cellContent);
            
            currentColumn++;
            if (currentColumn != assembliesColumn)
            {
                currentCell = row.ElementAt(currentColumn);
                if (!string.IsNullOrEmpty(TextInCell(currentCell)))
                {
                    do
                    {
                        component.Subcomponents.Add(BuildSpec(currentColumn, assembliesColumn));
                        if (rows.Count == 0)
                            break;
                        row = rows.First().Elements<Cell>().ToList();
                        currentCell = row.ElementAt(currentColumn);
                    } while (!string.IsNullOrEmpty(TextInCell(currentCell))
                            && string.IsNullOrEmpty(TextInCell(row.ElementAt(currentColumn - 1))));
                }
                else
                {
                    if (rows.Count > 1)
                    {
                        do
                        {
                            try
                            {
                                component.Assemblies.Add(TextInCell(row.ElementAt(assembliesColumn)));
                            }
                            catch
                            {
                                MessageBox.Show("Invalid specification file :\nPlease respect the spreadsheet pattern, you didn't specified an assembly");
                            }

                            rows.RemoveAt(0);
                            if (rows.Count == 0)
                                break;
                            row = rows.First().Elements<Cell>().ToList();
                            currentCell = row.ElementAt(currentColumn);
                        } while (string.IsNullOrEmpty(TextInCell(currentCell))
                                && string.IsNullOrEmpty(TextInCell(row.ElementAt(currentColumn - 1))));

                        if (!string.IsNullOrEmpty(TextInCell(currentCell))
                            && string.IsNullOrEmpty(TextInCell(row.ElementAt(currentColumn - 1))))
                        {
                            do
                            {
                                component.Subcomponents.Add(BuildSpec(currentColumn, assembliesColumn));
                                if (rows.Count == 0)
                                    break;
                                row = rows.First().Elements<Cell>().ToList();
                                currentCell = row.ElementAt(currentColumn);
                            } while (!string.IsNullOrEmpty(TextInCell(currentCell))
                                    && string.IsNullOrEmpty(TextInCell(row.ElementAt(currentColumn - 1))));
                        }
                    }
                }
            }
            else
            {
                currentColumn--;
                cellContent = null;
                while (string.IsNullOrEmpty(cellContent))
                {
                    try
                    {
                        cellContent = TextInCell(row.ElementAt(assembliesColumn));
                        if (!string.IsNullOrEmpty(cellContent))
                            component.Assemblies.Add(cellContent);
                        else
                            break;
                    }
                    catch
                    {
                        MessageBox.Show("Invalid specification file :\nPlease respect the spreadsheet pattern, you didn't specified an assembly");
                    }

                    rows.RemoveAt(0);
                    if (rows.Count == 0)
                        break;
                        
                    row = rows.First().Elements<Cell>().ToList();
                    cellContent = null;

                    foreach (Cell cell in row.Where(x => row.IndexOf(x) < assembliesColumn))
                    {
                        cellContent = TextInCell(cell);
                        if (!string.IsNullOrEmpty(TextInCell(cell)))
                            break;
                    }
                }
            }

            return component;
        }
    }
}
