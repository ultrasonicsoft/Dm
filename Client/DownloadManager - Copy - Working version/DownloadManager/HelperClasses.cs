using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using Ultrasonic.DownloadManager.Core.ViewModels;
using System.Reflection;
using Ultrasonic.DownloadManager.DownloadManagerService;

namespace Ultrasonic.DownloadManager
{
    #region Helpers Classes

    public static class ComboViewShowcaseHelper
    {
        public static List<MyComboViewModel> GetSource(int id)
        {
            List<MyComboViewModel> roots = new List<MyComboViewModel>();
            switch (id)
            {
                case 0:
                    FillCategories(roots);
                    break;
                case 1:
                    FillInformation(roots);
                    break;
            }
            return roots;
        }

        private static void FillCategories(List<MyComboViewModel> roots)
        {
            DownloadDataProviderServiceClient client = new DownloadDataProviderServiceClient();
            var categoryData = client.GetCategories();
            XDocument doc = XDocument.Parse(categoryData);

            //XDocument doc = XDocument.Load(XmlReader.Create(Helper.CATEGORIES_XML_PATH));

            var nodes = doc.Descendants(XName.Get("types"));

            MyComboViewModel f = new MyComboViewModel();
            MyComboViewModel f1 = new MyComboViewModel();

            foreach (var item in nodes)
            {
                f = new MyComboViewModel();
                f.Name = item.Attribute(XName.Get("name")).Value;
                foreach (var subItem in item.Descendants(XName.Get("item")))
                {
                    f1 = new MyComboViewModel();
                    f1.Name = subItem.Value;

                    if (subItem.HasAttributes)
                    {
                        // = subItem.Attribute(XName.Get("checked")).Value == "yes" ? true : false;
                    }

                    f.Children.Add(f1);
                }
                roots.Add(f);
            }
        }

        private static void FillInformation(List<MyComboViewModel> roots)
        {
            DownloadDataProviderServiceClient client = new DownloadDataProviderServiceClient();
            var categoryData = client.GetInformation();
            XDocument doc = XDocument.Parse(categoryData);

            //XDocument doc = XDocument.Load(XmlReader.Create(Helper.INFORMATION_XML_PATH));
            var nodes = doc.Descendants(XName.Get("item"));

            MyComboViewModel f = new MyComboViewModel();

            foreach (var item in nodes)
            {
                f = new MyComboViewModel();
                f.Name = item.Value;
                roots.Add(f);
            }
        }
    }

    public class MyComboViewModel : ViewModelBase
    {
        public int Id { get; set; }

        private string name;
        public string Name
        {
            get { return string.IsNullOrEmpty(name) ? string.Empty : name; }
            set { SetProperty(MethodBase.GetCurrentMethod(), ref name, value); }
        }

        public bool ShowCheck
        {
            get
            {
                return Id % 2 == 0;
            }
        }

        public List<MyComboViewModel> Children { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Name.Equals(((MyComboViewModel)obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public MyComboViewModel()
        {
            Children = new List<MyComboViewModel>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
    #endregion
}
