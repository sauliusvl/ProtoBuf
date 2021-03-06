using System;
using System.Collections.Generic;
using System.Linq;

namespace SilentOrbit.ProtocolBuffers
{
    static class Search
    {
        /// <summary>
        /// Search for message in hierarchy
        /// </summary>
        public static ProtoType GetProtoType(ProtoMessage msg, string path)
        {
            //Search for message or enum
            ProtoType pt;

            //Search from one level up until a match is found
            while (msg is ProtoCollection == false)
            {
                //Search sub messages
                pt = SearchSubMessages(msg, msg.Package + "." + msg.ProtoName + "." + path);
                if (pt != null)
                    return pt;

                var packages = new List<String>();

                packages.Add(msg.Package);
                
                // Search among included files

                var parentCollection = msg.Parent;
                while (parentCollection is ProtoCollection == false)
                    parentCollection = parentCollection.Parent;

                var includedMessages = parentCollection.Messages.Values.Where(m => msg.IncludedFiles.Contains(m.DefinitionFile));
                var includedPackages = includedMessages.Select(m => m.Package).Distinct();

                packages.AddRange(includedPackages);

                //Search siblings, including higher up in the package namespaces

                var packageParts = msg.Package.Split('.');
                
                for (int i = packageParts.Length - 1; i >= 1 ; --i)
                {
                    packages.Add(packageParts.Take(i).Aggregate((p1, p2) => p1 + "." + p2));
                }

                foreach (var package in packages)
                {
                    pt = SearchSubMessages(msg.Parent, package + "." + path);
                    if (pt != null)
                        return pt;
                }

                msg = msg.Parent;
            }

            //Finally search for global namespace
            return SearchSubMessages(msg, path);
        }

        static ProtoType SearchSubMessages(ProtoMessage msg, string fullPath)
        {
            foreach (ProtoMessage sub in msg.Messages.Values)
            {
                if (fullPath == sub.FullProtoName)
                    return sub;

                if (fullPath.StartsWith(sub.FullProtoName + "."))
                {
                    ProtoType pt = SearchSubMessages(sub, fullPath);
                    if (pt != null)
                        return pt;
                }
            }

            foreach (ProtoEnum subEnum in msg.Enums.Values)
            {
                if (fullPath == subEnum.FullProtoName)
                    return subEnum;
            }

            return null;
        }
    }
}

