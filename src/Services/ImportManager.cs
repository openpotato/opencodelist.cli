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

using Enbrea.Csv;
using Enbrea.Konsoli;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCodeList.Cli;

/// <summary>
/// Importer class for OpenCodeList documents
/// </summary>
public class ImportManager
{
    private readonly ConsoleWriter _consoleWriter;
    private readonly FileInfo _file;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportManager"/> class.
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    public ImportManager(FileInfo file)
    {
        _consoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
        _file = file;
    }

    /// <summary>
    /// Executes a data import from CSV
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    /// <param name="csvFile">The CSV file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    public static async Task FromCsvAsync(FileInfo file, FileInfo csvFile, CancellationToken cancellationToken)
    {
        var importManager = new ImportManager(file);
        await importManager.FromCsvAsync(csvFile, cancellationToken);
    }

    /// <summary>
    /// Executes a data import from CSV
    /// </summary>
    /// <param name="csvFile">The CSV file</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    public async Task FromCsvAsync(FileInfo csvFile, CancellationToken cancellationToken)
    {
        try
        {
            // Start...
            _consoleWriter.Caption($"Import to OpenCodeList from CSV...");

            // Open code list document
            var oclDocument = await CodeListDocument.LoadAsync(_file, cancellationToken);

            // Clear existing rows in code list document
            oclDocument.Rows.Clear();

            // Open CSV file stream
            using var strReader = new StreamReader(csvFile.FullName);

            // Create CSV reader
            var csvTableReader = new CsvTableReader(strReader, new CsvConfiguration { Separator = ',' });

            // Read CSV headers
            await csvTableReader.ReadHeadersAsync();

            // Read CSV rows and add them to code list document
            while (await csvTableReader.ReadAsync() > 0)
            {
                var row = oclDocument.Rows.Add();

                foreach (var column in oclDocument.Columns)
                {
                    row[column.Id] = csvTableReader[column.Id];
                }
            }

            // Save code list document 
            await oclDocument.SaveAsync(_file, cancellationToken);

            // Success!
            _consoleWriter.Success($"Import completed successfully.");
        }
        catch (Exception ex)
        {
            _consoleWriter.NewLine();
            _consoleWriter.Error($"Import failed. {ex.Message}");
            throw;
        }
    }
}
