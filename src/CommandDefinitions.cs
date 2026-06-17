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

using System.CommandLine;
using System.IO;

namespace OpenCodeList.Cli;

/// <summary>
/// Defines the command line commands and options for the OpenCodeList CLI application.
/// </summary>
public static class CommandDefinitions
{
    /// <summary>
    /// Defines the "export" command, which exports from a code list document to a supported format.
    /// </summary>
    /// <returns>The configured export command</returns>
    public static Command Export()
    {
        var command = new Command("export", "Exports from a code list document to a supported format")
        {
            new Option<FileInfo>("--file", "-f")
            {
                Description = "Specifies the code list document source file",
                Required = true
            },
            new Option<ExportFormat>("--to-format")
            {
                Description = "Specifies the destination file format",
                Required = true
            },
            new Option<FileInfo>("--to-file")
            {
                Description = "Specifies the destination file",
                Required = true
            }
        };

        command.SetAction(parseResult => CommandHandlers.Export(
            parseResult.GetValue(command.Options[0] as Option<FileInfo>),
            parseResult.GetValue(command.Options[1] as Option<ExportFormat>),
            parseResult.GetValue(command.Options[2] as Option<FileInfo>))
        );

        return command;
    }

    /// <summary>
    /// Defines the "import" command, which imports from a supported format to a code list document.
    /// </summary>
    /// <returns>The configured import command</returns>
    public static Command Import()
    {
        var command = new Command("import", "Imports to a code list document from a supported format")
        {
            new Option<FileInfo>("--file", "-f")
            {
                Description = "Specifies the code list document destination file",
                Required = true
            },
            new Option<ImportFormat>("--from-format")
            {
                Description = "Specifies the source file format",
                Required = true
            },
            new Option<FileInfo>("--from-file")
            {
                Description = "Specifies the source file",
                Required = true
            }
        };

        command.SetAction(parseResult => CommandHandlers.Import(
            parseResult.GetValue(command.Options[0] as Option<FileInfo>),
            parseResult.GetValue(command.Options[1] as Option<ImportFormat>),
            parseResult.GetValue(command.Options[2] as Option<FileInfo>))
        );

        return command;
    }
}
