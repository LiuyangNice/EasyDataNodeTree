// See https://aka.ms/new-console-template for more information
using EasyData;
using Newtonsoft.Json.Linq;

Console.WriteLine("Hello, World!");
DataCenter dataCenter = new DataCenter();
var dataNode = new DataNode("c", new DataBase() { Value = 10 });
var listener = new TestListener();
dataNode.AddNodeListener(listener);
dataCenter.AddOrUpdateDataNode("/a/b/c", dataNode);
var d = dataCenter.GetDataNode("/a/b/c", out var node);
node.UpdateDataNode(new DataBase() { Value = 11 });

dataCenter.AddOrUpdateDataNode("a/b/c/d", new DataNode("d", new DataBase() { Value = 12 }));


Console.WriteLine($"{d} {node.Data.Value}");
node.UpdateDataNode(new DataBase() { Value = 10 });
dataCenter.DeleteDataNode("a/b/c/d");

node.RemoveNodeListener(listener);
JToken token = new JObject()
{
    ["a"] = new JObject()
    {
        ["a1"] = 10,
        ["a2"] = new JArray() { 11, 12, 13, 14, 15 }
    },
    ["b"] = 10,
    ["c"] = new JArray() { 1, 2, 3, 4, 5 },
    ["d"] = false
};
dataCenter.DeleteDataNode("a/b/c/d");
dataCenter.LoadFromJson("/", token);
if (dataCenter.GetDataNode("a/a2/2", out var node1))
{
    Console.WriteLine(node1.Data.Value);
}
public class TestListener : IDataNodeListener
{
    public void OnAddChild(DataNode node)
    {
        Console.WriteLine($"添加子节点{node.Name}");
    }

    public void OnDataNodeChange(DataNode node)
    {
        Console.WriteLine($"更新数据{node.Data.Value}");
    }

    public void OnDeleteChild(DataNode node)
    {
        Console.WriteLine($"删除子节点{node.Name}");
    }
}