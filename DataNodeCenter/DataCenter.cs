using Newtonsoft.Json.Linq;
namespace EasyData
{
    public interface IDataNodeListener
    {
        void OnAddChild(DataNode node);
        void OnDataNodeChange(DataNode node);
        void OnDeleteChild(DataNode node);
    }

    public class DataCenter
    {
        private readonly DataNode m_Root = new("/");

        public bool GetDataNode(string path, out DataNode node)
        {
            var paths = path.Split("/");
            var currentNode = m_Root;
            foreach (var item in paths.Where(str => !string.IsNullOrEmpty(str)))
            {

                currentNode = currentNode.GetChild(item);
                if (currentNode == null)
                {
                    node = null;
                    return false;
                }
            }
            node = currentNode;
            return true;
        }

        public void LoadFromJson(string path, JToken token)
        {
            var subPath = "";
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var item in token as JObject)
                    {
                        subPath = $"{path}/{item.Key}";
                        LoadFromJson(subPath, item.Value);
                    }
                    break;
                case JTokenType.Array:
                    var array = token as JArray;
                    for (var i = 0; i < array?.Count; i++)
                    {
                        subPath = $"{path}/{i}";
                        LoadFromJson(subPath, array[i]);
                    }
                    break;
                case JTokenType.Property:
                    var property = token as JProperty;
                    subPath = $"{path}/{property?.Name}";
                    var data = new DataBase() { Value = property?.Value };
                    var node = new DataNode(property?.Name, data);
                    AddOrUpdateDataNode(subPath, node);
                    break;
                default:
                    var str = Path.GetFileName(path);
                    AddOrUpdateDataNode(path, new DataNode(str, new DataBase() { Value = token }));
                    break;
            }
        }
        public bool AddOrUpdateDataNode(string path, DataNode node)
        {
            if (GetDataNode(path, out var currentNode))
            {
                Console.WriteLine("当前节点已经存在");
                currentNode.UpdateDataNode(node.Data);
                return false;
            }
            else
            {
                var checkNode = m_Root;
                path = path.TrimEnd(node.Name.ToCharArray());
                var paths = path.Split("/");

                foreach (var item in paths.Where(str => !string.IsNullOrEmpty(str)))
                {
                    var cNode = checkNode.GetChild(item);
                    if (cNode == null)
                    {
                        cNode = new DataNode(item);
                        checkNode.AddChild(cNode);
                    }
                    checkNode = cNode;
                }
                checkNode.AddChild(node);

            }

            return true;
        }

        public bool DeleteDataNode(string path)
        {
            if (GetDataNode(path, out var node))
            {
                node.DeleteDataNode();
                return true;
            }
            return false;
        }

    }
    public class DataBase
    {
        public object Value;
    }
    public class DataNode
    {
        private readonly List<IDataNodeListener> m_NodeListeners;

        public DataNode(string name)
        {
            this.Name = name;
            m_NodeListeners = new List<IDataNodeListener>();
            m_Children = new Dictionary<string, DataNode>();
        }
        public DataNode(string name, DataBase data)
        {
            Name = name;
            m_NodeListeners = new List<IDataNodeListener>();
            m_Children = new Dictionary<string, DataNode>();
            Data = data;
        }
        public readonly string Name;
        private DataNode m_Parent;
        private readonly Dictionary<string, DataNode> m_Children;
        public DataBase Data;

        public void AddChild(DataNode node)
        {
            node.m_Parent = this;
            m_Children.Add(node.Name, node);
            foreach (var listener in m_NodeListeners)
            {
                listener.OnAddChild(node);
            }
        }

        public void AddNodeListener(IDataNodeListener listener)
        {
            m_NodeListeners.Add(listener);
        }

        public void RemoveNodeListener(IDataNodeListener listener)
        {
            m_NodeListeners.Remove(listener);
        }

        private void DeleteChild(DataNode node)
        {
            m_Children.Remove(node.Name);
            foreach (var listener in m_NodeListeners)
            {
                listener.OnDeleteChild(node);
            }
        }
        public DataNode GetChild(string childName)
        {
            m_Children.TryGetValue(childName, out var child);
            return child;
        }

        public void UpdateDataNode(DataBase dataBase)
        {
            Data = dataBase;
            foreach (var listener in m_NodeListeners)
            {
                listener.OnDataNodeChange(this);
            }
        }

        public void DeleteDataNode()
        {
            m_Parent.DeleteChild(this);
            foreach (var child in m_Children.Values)
            {
                child.DeleteDataNode();
            }
        }
    }
    }
