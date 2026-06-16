#region OpenCodeList.CLI - Copyright (C) STÜBER SYSTEMS GmbH
/*    
 *    OpenCodeList.CLI 
 *    
 *    Copyright (C) STÜBER SYSTEMS GmbH
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Affero General Public License, version 3,
 *    as published by the Free Software Foundation.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *    GNU Affero General Public License for more details.
 *
 *    You should have received a copy of the GNU Affero General Public License
 *    along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 */
#endregion

using ClosedXML.Excel;
using Enbrea.Csv;
using Enbrea.Konsoli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCodeList.Cli;

/// <summary>
/// Exporter class for OpenCodeList documents
/// </summary>
public class ExportManager
{
    private readonly ConsoleWriter _consoleWriter;
    private readonly FileInfo _file;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportManager"/> class.
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    public ExportManager(FileInfo file)
    {
        _consoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
        _file = file;
    }

    /// <summary>
    /// Executes a data export to CSV
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    /// <param name="csvFile">The CSV file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public static async Task ToCsvAsync(FileInfo file, FileInfo csvFile, CancellationToken cancellationToken)
    {
        var exportManager = new ExportManager(file);
        await exportManager.ToCsvAsync(csvFile, cancellationToken);
    }

    /// <summary>
    /// Executes a data export to Excel
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    /// <param name="xlsxFile">The Excel file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public static async Task ToXlsxAsync(FileInfo file, FileInfo xlsxFile, CancellationToken cancellationToken)
    {
        var exportManager = new ExportManager(file);
        await exportManager.ToXlsxAsync(xlsxFile, cancellationToken);
    }

    /// <summary>
    /// Executes a data export to CSV
    /// </summary>
    /// <param name="csvFile">The CSV file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task ToCsvAsync(FileInfo csvFile, CancellationToken cancellationToken)
    {
        try
        {
            // Start...
            _consoleWriter.Caption($"Export from OpenCodeList to CSV...");

            // Open code list document
            var codeList = await CodeListDocument.LoadAsync(_file, cancellationToken);

            // Build CSV headers
            var csvHeaders = new List<string>();

            foreach (var column in codeList.Columns)
            {
                csvHeaders.Add(column.Id);
            }

            // Create CSV file stream
            using var strWriter = new StreamWriter(csvFile.FullName);

            // Create CSV writer
            var csvTableWriter = new CsvTableWriter(strWriter, new CsvConfiguration { Separator = ',' }, csvHeaders);

            // Write CSV headers
            await csvTableWriter.WriteHeadersAsync();

            // Write CSV rows
            foreach (var row in codeList.Rows)
            {
                foreach (var column in codeList.Columns)
                {
                    csvTableWriter.SetValue(column.Id, row[column.Id]);
                }
                await csvTableWriter.WriteAsync();
            }

            // Success!
            _consoleWriter.Success($"Export completed successfully.");
        }
        catch (Exception ex)
        {
            _consoleWriter.NewLine();
            _consoleWriter.Error($"Export failed. {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Executes a data export to Excel
    /// </summary>
    /// <param name="xlsxFile">The Excel file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task ToXlsxAsync(FileInfo xlsxFile, CancellationToken cancellationToken)
    {
        try
        {
            // Start...
            _consoleWriter.Caption($"Export from OpenCodeList to Excel...");

            // Open code list document
            var codeList = await CodeListDocument.LoadAsync(_file, cancellationToken);

            // Create Excel workbook and worksheet
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Code List");

            // ----- Metadata section -----
            var xlsRow = 1;

            void AddRow(string label, object value)
            {
                ws.Cell(xlsRow, 1).Value = label;
                ws.Cell(xlsRow, 2).Value = value?.ToString();
                ws.Cell(xlsRow, 1).Style.Font.Bold = true;
                ws.Cell(xlsRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                xlsRow++;
            }

            void AddDateTimeRow(string label, DateTimeOffset value)
            {
                ws.Cell(xlsRow, 1).Value = label;
                ws.Cell(xlsRow, 2).Value = value.LocalDateTime;
                ws.Cell(xlsRow, 1).Style.Font.Bold = true;
                ws.Cell(xlsRow, 2).Style.DateFormat.Format = "yyyy-mm-ddThh:mm:ss";
                ws.Cell(xlsRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                xlsRow++;
            }

            AddRow("ShortName", codeList.Identification.ShortName);

            if (!string.IsNullOrWhiteSpace(codeList.Identification.LongName))
            {
                AddRow("LongName", codeList.Identification.LongName);
            }

            if (!string.IsNullOrWhiteSpace(codeList.Identification.Version))
            {
                AddRow("Version", codeList.Identification.Version);
            }

            if (!string.IsNullOrWhiteSpace(codeList.Identification.Language))
            {
                AddRow("Language", codeList.Identification.Language);
            }

            if (codeList.Identification.CanonicalUri is not null)
            {
                AddRow("CanonicalUri", codeList.Identification.CanonicalUri);
            }

            if (codeList.Identification.CanonicalVersionUri is not null)
            {
                AddRow("CanonicalVersionUri", codeList.Identification.CanonicalVersionUri);
            }

            if (codeList.Identification.Tags.Count > 0)
            {
                AddRow("Tags", string.Join(", ", codeList.Identification.Tags));
            }

            if (codeList.Identification.ValidFrom is not null)
            {
                AddDateTimeRow("ValidFrom", codeList.Identification.ValidFrom.Value);
            }

            if (codeList.Identification.ValidTo is not null)
            {
                AddDateTimeRow("ValidTo", codeList.Identification.ValidTo.Value);
            }

            if (codeList.Identification.PublishedAt is not null)
            {
                AddDateTimeRow("PublishedAt", codeList.Identification.PublishedAt.Value);
            }

            if (codeList.Identification.Publisher is not null)
            {
                AddRow("Publisher.ShortName", codeList.Identification.Publisher.ShortName);

                if (!string.IsNullOrWhiteSpace(codeList.Identification.Publisher.LongName))
                {
                    AddRow("Publisher.LongName", codeList.Identification.Publisher.LongName);
                }

                if (codeList.Identification.Publisher.Url is not null)
                {
                    AddRow("Publisher.Url", codeList.Identification.Publisher.Url.ToString());
                }
            }

            xlsRow++;

            // ----- Table section -----
            var xlsTableStartRow = xlsRow;

            for (var idx = 0; idx < codeList.Columns.Count; idx++)
            {
                var column = codeList.Columns[idx];
                ws.Cell(xlsRow, idx + 1).Value = string.IsNullOrEmpty(column.Name) ? column.Id : column.Name;
            }

            xlsRow++;

            foreach (var row in codeList.Rows)
            {
                for (var idx = 0; idx < codeList.Columns.Count; idx++)
                {
                    var column = codeList.Columns[idx];

                    if (column is StringColumn stringColum && row[column.Id] is string stringValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = stringValue;
                    }
                    else if (column is BooleanColumn booleanColumn && row[column.Id] is bool boolValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = boolValue;
                    }
                    else if (column is IntegerColumn integerColumn && row[column.Id] is int intValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = intValue;
                    }
                    else if (column is NumberColumn numberColumn && row[column.Id] is double doubleValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = doubleValue;
                    }
                    else if (column is EnumColumn enumColumn && row[column.Id] is string enumValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = enumValue;
                    }
                    else if (column is EnumColumn enumSetColumn && row[column.Id] is string[] enumSetValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = string.Join(", ", enumSetValue);
                    }
                    else if (column is DateOnlyColumn dateOnlyColumn && row[column.Id] is DateOnly dateOnlyValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = dateOnlyValue.ToDateTime(TimeOnly.MinValue);
                        ws.Cell(xlsRow, idx + 1).Style.DateFormat.Format = "yyyy-mm-dd";
                    }
                    else if (column is TimeOnlyColumn timeOnlyColumn && row[column.Id] is TimeOnly timeOnlyValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = DateOnly.FromDateTime(DateTime.Today).ToDateTime(timeOnlyValue); ;
                        ws.Cell(xlsRow, idx + 1).Style.DateFormat.Format = "HH:mm:ss";
                    }
                    else if (column is DateTimeColumn dateTimeColum && row[column.Id] is DateTimeOffset dateTimeValue)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = dateTimeValue.LocalDateTime;
                        ws.Cell(xlsRow, idx + 1).Style.DateFormat.Format = "yyyy-mm-ddThh:mm:ss";
                    }
                    else if (column is JsonColumn jsonColumn && row[column.Id] is JsonElement jsonElement)
                    {
                        ws.Cell(xlsRow, idx + 1).Value = jsonElement.ToString();
                    }
                }

                xlsRow++;
            }

            // Create Excel table
            var tableRange = ws.Range(xlsTableStartRow, 1, xlsRow - 1, codeList.Columns.Count);
            var table = tableRange.CreateTable("CodeListTable");

            // Enables Excel filter/sort dropdowns
            table.ShowAutoFilter = true;

            // Styling
            table.Theme = XLTableTheme.TableStyleMedium2;
            ws.Columns().AdjustToContents();

            // Freeze table header
            ws.SheetView.FreezeRows(xlsTableStartRow);

            // Save file
            workbook.SaveAs(xlsxFile.FullName);

            // Success!
            _consoleWriter.Success($"Export completed successfully.");
        }
        catch (Exception ex)
        {
            _consoleWriter.NewLine();
            _consoleWriter.Error($"Export failed. {ex.Message}");
            throw;
        }
    }
}
