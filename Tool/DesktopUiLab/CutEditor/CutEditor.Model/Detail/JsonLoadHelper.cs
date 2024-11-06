namespace CutEditor.Model.Detail;

using System.Collections.Generic;
using System.Drawing;
using Cs.Core.Util;
using Newtonsoft.Json.Linq;

internal static class JsonLoadHelper
{
    public static Color? LoadColor(JToken token, string key)
    {
        var buffer = new List<float>(); // note: ���� json ������ element�� string���� �Ǿ� �ֽ��ϴ�. 
        if (token.TryGetArray(key, buffer, (token, i) => float.Parse(token.ToString())) == false)
        {
            return null;
        }

        if (buffer.Count == 3)
        {
            // ������ alpha�� 0���� ä��. 
            buffer.Add(0);
        }

        if (buffer.Count != 4)
        {
            return null;
        }

        // �����Ϳ��� RGBA ������ ����ְ�, �Ʒ� �����ڴ� ARGB ������ �޽��ϴ�.
        return Color.FromArgb(
            alpha: (byte)(buffer[3] * 255),
            red: (byte)(buffer[0] * 255),
            green: (byte)(buffer[1] * 255),
            blue: (byte)(buffer[2] * 255));
    }
}
