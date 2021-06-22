using System;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace Obfuscation
{
    public partial class Form1 : Form
    {
        bool isOpen = false, isObfuscated = false;
        string comPath;
        string fileNameExt;

        List<String> variables = new List<String> ();
        string[] exeptStrings = { "using", "namespace", "static", "class", "new", "else",
        "return", "protected", "public" };
        string[] dataTypes = { "var", "void", "int", "char", "bool", "String", "string", "float", "double" };

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"c:\\Users\Oleksandr\Desktop";
                openFileDialog.Filter = "CS files (*.cs)|*.cs|C++ files (*.cpp)|*.cpp";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    comPath = filePath;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();
                    fileNameExt = openFileDialog.SafeFileName;

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                        textBox1.Text = fileContent;
                        isOpen = true;
                    }
                    textBox2.Text = " ";
                }
            }
        }

        private void ObfusAll() 
        {
            string formatText = textBox1.Text;
            int start;
            while (formatText.Contains("//"))
            {
                start = formatText.IndexOf("//");
                formatText = formatText.Remove(start,
                    formatText.IndexOf(Environment.NewLine, start) - start + 2);
            }
            progressBar1.PerformStep();
            while (formatText.Contains("/*"))
            {
                start = formatText.IndexOf("/*");
                formatText = formatText.Remove(start,
                    formatText.IndexOf("*/", start) - start + 2);
            }
            progressBar1.PerformStep();

            FindClasses(formatText);
            progressBar1.PerformStep();
            for (int i = 0; i < dataTypes.Length - 1; i++)
            {
                FindVars(formatText, dataTypes[i]);
                FindArrs(formatText, dataTypes[i]);
            }
            for (int i = 0; i < variables.Count - 1; i++) {
                if (variables[i].Contains("(") || variables[i] == "")
                    variables.RemoveAt(i);
            }
            progressBar1.PerformStep();

            for (int i = 0; i < variables.Count-1; i++)
            {
                Random rnd = new Random();
                String addVal = "", previousNewValue = "", newValue = genName();
                if (newValue == previousNewValue)
                {
                    addVal = ((char)rnd.Next(97, 123)).ToString();
                    newValue += addVal;
                }
                if (i / variables.Count == 0)
                    progressBar1.PerformStep();
                if (i+1 / variables.Count == 1)
                    progressBar1.PerformStep();
                if (formatText.Contains(variables[i]))
                    formatText = formatText.Replace(variables[i], previousNewValue = newValue);
            }
            progressBar1.PerformStep();
            progressBar1.PerformStep();  

            formatText = regularStrings(formatText);
            formatText = formatText.Replace(Environment.NewLine, String.Empty);
            progressBar1.PerformStep();

            for (int i = 0; i < exeptStrings.Length; i++)
                formatText = execeptions(formatText, exeptStrings[i]);
            progressBar1.PerformStep();
            for (int i = 0; i < dataTypes.Length; i++)
                formatText = execeptions(formatText, dataTypes[i]);
            textBox2.Text = formatText;
            progressBar1.PerformStep();
            isObfuscated = true;
        }

        private string execeptions(string mainString, string exeption)
        {
            String noR = mainString, keyword = exeption; 
            if (noR.Contains(keyword))
                noR = noR.Replace(keyword, keyword + " ");

            return noR;
        }

        private string genName() {
            Random rnd = new Random();
            Thread.Sleep(50);
            int way, length = rnd.Next(1, 7);
            int preResult;
            String result = "";

            for (int i = 0; i < length; i++)
            {
                way = rnd.Next(1, 3);
                if (way==1)
                    preResult = rnd.Next(65, 91);
                else
                    preResult = rnd.Next(97, 123);
                result += (char) preResult;
            }
            return result;
        }

        private void FindVars(string origin, string type) 
        {
            String noR = origin;
            String ftype = " " + type + " ";
            if (noR.Contains(ftype))
            {
                String newStr = noR;
                String s1 = "";

                while (newStr.Contains(ftype))
                {
                    s1 = newStr.Substring(newStr.IndexOf(ftype) + type.Length + 2,
                        newStr.IndexOf("=", newStr.IndexOf(ftype)) - newStr.IndexOf(ftype));
                    s1 = s1.Replace(";", " ");
                    s1 = s1.Replace(")", " ");
                    s1 = s1.Replace("(", " ");
                    s1 = s1.Replace("[", " ");
                    s1 = s1.Replace("]", " ");
                    s1 = s1.Replace(Environment.NewLine, " ");
                    s1 = s1.Substring(0, s1.IndexOf(" "));
                    if (s1 != "Main")
                        variables.Add(s1);
                    newStr = newStr.Substring(newStr.IndexOf(ftype) + 5);               
                }
            }
        }

        private void FindClasses(string origin)
        {
            String noR = origin;
            if (noR.Contains(" "+"class"+" "))
            {
                String newStr = noR;
                String s1 = "";

                while (newStr.Contains(" " + "class" + " "))
                {
                    s1 = newStr.Substring(newStr.IndexOf(" " + "class" + " ") + 5 + 2,
                        newStr.IndexOf("(", newStr.IndexOf(" " + "class" + " ")) - newStr.IndexOf(" " + "class" + " "));
                    s1 = s1.Replace("(", " ");
                    s1 = s1.Replace(Environment.NewLine, " ");
                    s1 = s1.Substring(0, s1.IndexOf(" "));
                    variables.Add(s1);
                    newStr = newStr.Substring(newStr.IndexOf(" " + "class" + " ") + 5);
                }
            }
        }

        private void FindArrs(string origin, string type) 
        {
            String noR = origin;
            String ftype = " " + type;
            if (noR.Contains(ftype))
            {
                String newStr = noR;
                String s1 = "";

                if (newStr.Contains(type + "[]") || newStr.Contains(type + " []"))
                {
                    while (newStr.Contains(ftype) || newStr.Contains(type + "[]"))
                    {
                        s1 = newStr.Substring(newStr.IndexOf(ftype) + type.Length + 4,
                            newStr.IndexOf("=", newStr.IndexOf(ftype)) - newStr.IndexOf(ftype));
                        s1 = s1.Replace("[", " ");
                        s1 = s1.Replace(",", " ");
                        s1 = s1.Replace(";", " ");
                        s1 = s1.Replace("]", " ");
                        s1 = s1.Substring(0, s1.IndexOf(" "));
                        variables.Add(s1);
                        newStr = newStr.Substring(newStr.IndexOf(ftype) + 5);
                    }
                }
            }
        }

        private string regularStrings(string origin)
        {
            String noObfStr = "", noR = origin; 
            int start, finish;

            if (noR.Contains(" "))
            {
                String s1 = "", newStr = noR;
                noR = "";

                while (newStr.Contains(" ") || newStr.Contains("\""))
                {
                    mark: 
                    noR += deleteSpace(s1) + noObfStr;
                    if (newStr.Contains("\"") || newStr.Contains(" "))
                    {
                        start = newStr.IndexOf("\"");
                        finish = newStr.IndexOf("\"", start + 1);
                        if (start >= 0 && finish > 0)
                            noObfStr = newStr.Substring(start, finish - start + 1);
                        else
                            noObfStr = "";

                        if (!newStr.Contains("\""))
                        {
                            s1 = newStr.Substring(0);
                            newStr = "";
                            goto mark;
                        }
                        else
                        {
                            s1 = newStr.Substring(0, newStr.IndexOf(noObfStr));
                            newStr = newStr.Substring(newStr.IndexOf(noObfStr)-1 + noObfStr.Length + 1);
                        }
                    }
                    else
                        break;
                }
            }
            return noR;
        }

        private string deleteSpace(string origin) {
            String source = origin;
            source = source.Replace(" ", String.Empty);
            source = Regex.Replace(source, @"\s", "");

            return source;
        }

        private void обфускаціяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            if (isOpen)
            {
                ObfusAll();
            }
            else
                MessageBox.Show("Open file first", "Abort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isOpen && isObfuscated)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "CS files (*.cs)|*.cs|C++ files (*.cpp)|*.cpp";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = "Obf-" + fileNameExt;
                saveFileDialog1.AddExtension = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FilterIndex == 1)
                        saveFileDialog1.DefaultExt = ".cs";
                    else
                        saveFileDialog1.DefaultExt = ".cpp";
                    // создаем каталог для файла
                    string path = comPath.Substring(0, comPath.LastIndexOf("\\"));
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }

                    // запись в файл
                    string fullDirWrite = saveFileDialog1.FileName;
                    using (FileStream fstream = new FileStream(fullDirWrite, FileMode.OpenOrCreate))
                    {
                        byte[] array = System.Text.Encoding.Default.GetBytes(textBox2.Text);
                        // асинхронная запись массива байтов в файл
                        await fstream.WriteAsync(array, 0, array.Length);
                    }
                }
            }
            else if (!isOpen)
                MessageBox.Show("Open file first", "Abort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (!isObfuscated)
                MessageBox.Show("Obfuscate program first", "Abort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void aboutProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Symbolic Obfuscator v 1.0\nCopyright ©  2021 Oleksandr Palamarchuk\n" +
                "oleksandr.palamarchuk1310@gmail.com", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}