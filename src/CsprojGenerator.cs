using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class CsprojGenerator
    {
        public static async Task Generate(PackageReferenceModel model, TextWriter writer)
        {
            var xml = GenerateXml(model);
            using var w = XmlWriter.Create(writer,
                new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    Async = true
                }
            );
            await xml.WriteToAsync(w, CancellationToken.None);
            await writer.WriteLineAsync();
        }

        private static XDocument GenerateXml(PackageReferenceModel model)
        {
            var projectElement = new XElement("Project");

            if (model.Sdks.Length == 1)
            {
                projectElement.SetAttributeValue("Sdk", model.Sdks[0]);
            }

            if (model.Sdks.Length != 1)
            {
                foreach (var sdk in model.Sdks)
                {
                    projectElement.Add(new XElement("Sdk", new XAttribute("Name", sdk)));
                }
            }

            var propertyGroup = new XElement("PropertyGroup");
            projectElement.Add(propertyGroup);

            switch (model.TargetFrameworks.Length)
            {
                case 0:
                    break;
                case 1:
                    propertyGroup.Add(new XElement("TargetFramework", model.TargetFrameworks[0]));
                    break;
                default:
                    propertyGroup.Add(new XElement("TargetFrameworks", string.Join(";", model.TargetFrameworks)));
                    break;
            }

            switch (model.RuntimeIdentifiers.Length)
            {
                case 0:
                    break;
                case 1:
                    propertyGroup.Add(new XElement("RuntimeIdentifier", model.RuntimeIdentifiers[0]));
                    break;
                default:
                    propertyGroup.Add(new XElement("RuntimeIdentifiers", string.Join(";", model.RuntimeIdentifiers)));
                    break;
            }

            propertyGroup.Add(new XElement("_PackageCacheHash", model.Hash));

            foreach (var packageReferenceGroup in model.PackageReferenceGroups)
            {
                var itemGroup = new XElement("ItemGroup");
                projectElement.Add(itemGroup);

                var condition = $"'$(TargetFramework)' == '{packageReferenceGroup.TargetFramework}'";
                itemGroup.SetAttributeValue("Condition", condition);
                foreach (var packageReference in packageReferenceGroup.PackageReferences)
                {
                    itemGroup.Add(new XElement(
                            "PackageReference",
                            new XAttribute("Include", packageReference.Id),
                            new XAttribute("Version", packageReference.Version)
                        )
                    );
                }
            }
            /*
             *

                        foreach (var packageReferenceGroup in model.PackageReferenceGroups)
                        {
                            var condition = $"'$(TargetFramework)' == '{packageReferenceGroup.TargetFramework}'";
                            writer.WriteLine($"    <ItemGroup Condition=\"{condition}\">");
                            foreach (var packageReference in packageReferenceGroup.PackageReferences)
                            {
                                writer.WriteLine($"        <PackageReference Include=\"{packageReference.Id}\" Version=\"{packageReference.Version}\" />");
                            }
                            writer.WriteLine($"    </ItemGroup>");
                        }


                        writer.WriteLine($"</Project>");
             */

            return new XDocument(
                new XComment("NuGet package cache"),
                projectElement
            );
        }
    }
}