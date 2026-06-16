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

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCodeList.Cli;

/// <summary>
/// Command handlers for the OpenCodeList CLI application.
/// </summary>
public static class CommandHandlers
{
    /// <summary>
    /// Executes the export command, which exports from an OpenCodeList file to a supported format.
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    /// <param name="toFormat">The export format</param>
    /// <param name="toFile">The destination file</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public static async Task Export(FileInfo file, ExportFormat toFormat, FileInfo toFile)
    {
        await Execute(async (cancellationToken) =>
        {
            switch (toFormat)
            {
                case ExportFormat.Csv:
                    await ExportManager.ToCsvAsync(file, toFile, cancellationToken);
                    break;
                case ExportFormat.Xlsx:
                    await ExportManager.ToXlsxAsync(file, toFile, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported destination file format: {toFormat}");
            }
        });
    }

    /// <summary>
    /// Executes the import command, which imports from a supported format to an OpenCodeList file.
    /// </summary>
    /// <param name="file">The OpenCodeList document file</param>
    /// <param name="fromFormat">The import format</param>
    /// <param name="fromFile">The source file</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static async Task Import(FileInfo file, ImportFormat fromFormat, FileInfo fromFile)
    {
        await Execute(async (cancellationToken) =>
        {
            switch (fromFormat)
            {
                case ImportFormat.Csv:
                    await ImportManager.FromCsvAsync(file, fromFile, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported source file format: {fromFormat}");
            }
        });
    }

    /// <summary>
    /// Executes a given action with a cancellation token and measures the execution time.
    /// </summary>
    /// <remarks>
    /// The action is expected to be an asynchronous operation that can be cancelled. The method also handles cancellation requests 
    /// from the console (e.g., Ctrl+C) and ensures that the execution time is displayed after the action completes or is cancelled.
    /// </remarks>
    private static async Task Execute(Func<CancellationToken, Task> action)
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        var stopwatch = new Stopwatch();

        stopwatch.Start();

        try
        {
            await action(cancellationTokenSource.Token);
        }
        catch 
        {
            Environment.ExitCode = 1;
        }

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}.");
        Console.WriteLine();
    }
}