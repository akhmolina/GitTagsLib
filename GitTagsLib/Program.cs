using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using LibGit2Sharp;

namespace GitTagsLib
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string dataFormat = "r";
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            DialogResult result = fileDialog.ShowDialog();
            if (result != DialogResult.OK)
            { MessageBox.Show("Папка не выбрана."); Environment.Exit(0); }

            string repoPath = fileDialog.SelectedPath;
            Repository repo = null;

            try {repo = new Repository(repoPath);}
            catch (Exception ex)
            { MessageBox.Show("Не удалось создать репозиторий.\n" + ex.Message); Environment.Exit(0); }

            using (repo)
            {
                var tags = repo.Tags.Select(
                    t => new { name = t.FriendlyName.ToString(),
                            date = ((Commit)t.Target).Author.When.ToString(dataFormat, CultureInfo.InvariantCulture) });
                if (tags.Count() > 0)
                {
                    XmlDocument tagsXML = new XmlDocument();
                    XmlNode root = tagsXML.CreateNode(XmlNodeType.Element, "root", "");
                    tagsXML.AppendChild(root);

                    foreach (var tag in tags)
                    {
                        XmlNode xmltag = tagsXML.CreateNode(XmlNodeType.Element, "tag", "");

                        XmlAttribute nameAttribute = tagsXML.CreateAttribute("name");
                        nameAttribute.Value = tag.name;
                        xmltag.Attributes.Append(nameAttribute);

                        XmlAttribute dateAttribute = tagsXML.CreateAttribute("date");
                        dateAttribute.Value = tag.date;
                        xmltag.Attributes.Append(dateAttribute);

                        root.AppendChild(xmltag);
                    }
                    try
                    {
                        tagsXML.Save(repoPath + @"\..\tags.xml");
                    }
                    catch (Exception ex)
                    { MessageBox.Show("Не удалось сохранить файл xml.\n" + ex.Message); }
                }
                else { MessageBox.Show("В репозитории нет тегов."); }
            }
        }
    }
}
