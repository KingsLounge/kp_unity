using Newtonsoft.Json.Linq;

[System.Serializable]
public class Packet
{
    public int p;
    public JObject c;
    public string s = "";

    public Packet(int protocol)
    {
        p = protocol;
        c = new JObject();
    }

    public Packet(CPProtocol protocol, JObject c)
    {
        p = (int)protocol;
        this.c = c;
    }

    public Packet(PCProtocol protocol, JObject c)
    {
        p = (int)protocol;
        this.c = c;
    }

    public Packet(CPProtocol protocol)
    {
        p = (int)protocol;
        c = new JObject();
    }

    public Packet(PCProtocol protocol)
    {
        p = (int)protocol;
        c = new JObject();
    }

    public void Add(string key, JToken value)
    {
        c.Add(key, value);
    }

    public string ToJson()
    {
        JObject json = new JObject();
        json.Add("p", p);
        json.Add("c", c);
        return json.ToString();
    }
}
