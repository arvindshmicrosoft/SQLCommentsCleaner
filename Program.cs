//------------------------------------------------------------------------------
//<copyright company="Arvind Shyamsundar">
//    The MIT License (MIT)
//    
//    Copyright (c) 2018 Arvind Shyamsundar
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
//
//    This sample code is not supported under any Microsoft standard support program or service. 
//    The entire risk arising out of the use or performance of the sample scripts and documentation remains with you. 
//    In no event shall Microsoft, its authors, or anyone else involved in the creation, production, or delivery of the scripts
//    be liable for any damages whatsoever (including, without limitation, damages for loss of business profits,
//    business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability
//    to use the sample scripts or documentation, even if Microsoft has been advised of the possibility of such damages.
//</copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples
{
    using CommandLine;
    using Microsoft.SqlServer.TransactSql.ScriptDom;
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Program
    {
        static int Main(string[] args)
        {
            var parseResult = CommandLine.Parser.Default.ParseArguments<CmdLineOptions>(args)
                .MapResult(
                (CmdLineOptions opts) => {
                    return (CleanSQLScript(opts.SourceFile, opts.DestFile, opts.CompatLevel) ? 0 : 1);
                },
                errs => 1);

            return parseResult;
        }

        /// <summary>
        /// A simple T-SQL comment cleaner based on the SQLDOM that ships with SQL Management Studio / DACFX.
        /// After parsing the input file, we just loop through the input token stream, ignore any comment related token 
        /// and write out the other tokens to the output file.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="compatLevel"></param>
        /// <returns></returns>
        static bool CleanSQLScript(
            string sourceFile,
            string destFile,
            int compatLevel)
        {
            using (var srcRdr = new StreamReader(sourceFile))
            {
                TSqlParser parser;

                switch (compatLevel)
                {
                    case 80:
                        {
                            // SQL 2000
                            parser = new TSql80Parser(true);
                            break;
                        }

                    case 90:
                        {
                            // SQL 2005
                            parser = new TSql90Parser(true);
                            break;
                        }

                    case 100:
                        {
                            // SQL 2008 / R2
                            parser = new TSql100Parser(true);
                            break;
                        }
                    case 110:
                        {
                            // SQL 2012
                            parser = new TSql110Parser(true);
                            break;
                        }
                    case 120:
                        {
                            // SQL 2014
                            parser = new TSql120Parser(true);
                            break;
                        }
                    case 130:
                        {
                            // SQL 2016
                            parser = new TSql130Parser(true);
                            break;
                        }
                    case 140:
                        {
                            // SQL 2017
                            parser = new TSql140Parser(true);
                            break;
                        }
                    default:
                        {
                            parser = new TSql110Parser(true);
                            break;
                        }
                }

                IList<ParseError> errors;
                var tree = parser.Parse(srcRdr, out errors);

                if (errors.Count > 0)
                {
                    // TODO report parse errors
                    Console.WriteLine("Errors when parsing T-SQL");
                    return false;
                }

                using (var writer = new StreamWriter(destFile))
                {
                    foreach (var tok in tree.ScriptTokenStream)
                    {
                        // ignore all comments
                        if (tok.TokenType != TSqlTokenType.MultilineComment &&
                            tok.TokenType != TSqlTokenType.SingleLineComment)
                        {
                            writer.Write(tok.Text);
                        }
                    }

                    writer.Flush();
                    writer.Close();
                }                    
            }

            return true;
        }
    }
}
