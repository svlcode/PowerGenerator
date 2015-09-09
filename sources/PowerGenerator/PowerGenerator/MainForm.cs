using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PowerGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            var filePath = GetFilePath();
            if(!string.IsNullOrWhiteSpace(filePath))
            {
                if(File.Exists(filePath))
                {
                    txtFile.Text = filePath;
                    txtTableName.Text = GetTableNameFromFilePath(filePath);
                }
                else
                {
                    MessageBox.Show("Error - File does not exists!");
                }
            }
            else
            {
                MessageBox.Show("Error - Select a text file!");
            }
        }

        private string GetTableNameFromFilePath(string filePath)
        {
            var fileName = new FileInfo(filePath).Name;
            return "dbo." + fileName.Replace(".txt", "");
        }

        private string GetFilePath()
        {
            using (var dlg = new OpenFileDialog())
            {
                return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                if (!string.IsNullOrWhiteSpace(txtFile.Text))
                {
                    GenerateInsertScripts(txtFile.Text);
                }
            }
            
        }

        private void GenerateInsertScripts(string fileName)
        {
            var headers = ReadHeaders(fileName);
            var dataRecords = ReadDataRecords(fileName);
            var insertScripts = GenerateScripts(headers, dataRecords);
            rtbResult.Text = insertScripts;
        }

        private string GenerateScripts(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> dataRecords)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var dataRecord in dataRecords)
            {
                var inserScript = GetInsertScript(headers, dataRecord);
                builder.Append(inserScript);
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        private string GetInsertScript(IEnumerable<string> headers, IEnumerable<string> dataRecord)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GenerateInsertFieldsPart(headers.ToList()));
            builder.Append(GenerateInsertValuesPart(dataRecord.ToList()));
            return builder.ToString();
        }

        private string GenerateInsertValuesPart(List<string> dataRecord)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("VALUES (");
            builder.Append(Environment.NewLine);
            for (int i = 0; i < dataRecord.Count; i++)
            {
                var value = dataRecord[i];
                double number;
                if (!double.TryParse(value, out number))
                {
                    value = "'" + value + "'";
                }
                builder.Append(value);
                if (i < dataRecord.Count - 1)
                {
                    builder.Append(",");
                }
                builder.Append(Environment.NewLine);
            }
            builder.Append(")");
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        private string GenerateInsertFieldsPart(List<string> headers)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("INSERT INTO {0} (", txtTableName.Text));
            for (int i = 0; i < headers.Count; i++)
            {
                builder.Append(headers[i]);
                if(i < headers.Count -1)
                {
                    builder.Append(",");
                }
                builder.Append(Environment.NewLine);
            }
            builder.Append(")");
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        private IEnumerable<IEnumerable<string>> ReadDataRecords(string fileName)
        {
            List<IEnumerable<string>> dataLines = new List<IEnumerable<string>>();
            var rawLines = File.ReadAllLines(fileName);
            for (int i = 1; i < rawLines.Length; i++)
            {
                var line = GetDataRecord(rawLines[i]);
                dataLines.Add(line);
            }
            return dataLines;
        }

        private IEnumerable<string> GetDataRecord(string rawLine)
        {
            List<string> dataRecord = new List<string>();
            var lines = Regex.Split(rawLine, ",");
            var processedLines = GetProcessedLines(lines);
            dataRecord.AddRange(processedLines);
            return dataRecord;
        }

        private IEnumerable<string> GetProcessedLines(IEnumerable<string> rawLines)
        {
            List<string> lines = new List<string>();
            foreach (var rawLine in rawLines)
            {
                var processedLine = GetProcessedLine(rawLine);
                lines.Add(processedLine);
            }
            return lines;
        }

        private string GetProcessedLine(string rawLine)
        {
            var line = rawLine.Trim('\t');
            line = line.Trim();
            return line;
        }

        private IEnumerable<string> ReadHeaders(string fileName)
        {
            List<string> list = new List<string>();

            var firstLine = File.ReadLines(fileName).FirstOrDefault();
            if(firstLine != null)
            {
                var rawLines = Regex.Split(firstLine, ",");
                var processedLines = GetProcessedLines(rawLines);
                list.AddRange(processedLines);
            }
            return list;
        }

    }
}
