using System.Collections.Generic;
using LitJson;
using Newtonsoft.Json.Linq;

class JsonDataParser {
    public static void Parse(JsonData data,out int output) {
        output = int.Parse(data.ToString());
    }
    
    public static void Parse(JsonData data,out string output) {
        output = data.ToString();
    }

    public static void Parse(JsonData data,out long output) {
        output = long.Parse(data.ToString());
    }

    public static void Parse(JsonData data,out bool output) {
        output = bool.Parse(data.ToString());
    }
    public static void Parse(JsonData data,out float output) {
        output = float.Parse(data.ToString());
    }
    public static void Parse(JsonData data,out double output) {
        output = double.Parse(data.ToString());
    }
    public static void Parse(JsonData data,out JsonData[] output) {
        JsonData[] result = new JsonData[data.Count];
        for(int i = 0; i < data.Count; i++) {
            result[i] = data[i];
        }
        output = result;
    }
    public static void Parse(JsonData data,out string[] output) {
        string[] result = new string[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }

    public static void Parse(JsonData data,out int[] output) {
        int[] result = new int[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }

    public static void Parse(JsonData data,out long[] output) {
        long[] result = new long[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }

    public static void Parse(JsonData data,out bool[] output) {
        bool[] result = new bool[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }

    public static void Parse(JsonData data,out float[] output) {
        float[] result = new float[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }

    public static void Parse(JsonData data,out double[] output) {
        double[] result = new double[data.Count];
        for(int i = 0; i < data.Count; i++) {
            Parse(data[i],out result[i]);
        }
        output = result;
    }
    
    public static void Parse(JsonData data,out List<JsonData> output) {
        JsonData[] arr;
        Parse(data,out arr);
        output = new List<JsonData>(arr);
    }

    //=============================NewtonJson====================================
    
    public static T Parse<T>(JToken data)
    {
        return data.ToObject<T>();
    }
}