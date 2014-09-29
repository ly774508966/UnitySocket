using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Message_Head {
	public short HEADER { get; set; }              //��־����
    public short Length { get; set; }	           //��Ϣ�ֽڳ���
	public short CHECK_SUM { get; set; }           //��Ϣ�����
    public short ProtoVersion { get; set; }        //Э���
	public int  TOID { get; set; }                 //�ͻ���ID,Ĭ��Ϊ������Ϣ��ID
	public int EXTID1  { get; set; }               //һ����չ�ֶΣ�Ĭ��Ϊ0
    public int EXTID2 { get; set; }                //����һ����չ�ֶΣ�Ĭ��Ϊ0
	
}

public class Message_Body {
	public int iCommand { get; set; }
	public byte[] buffBytes { get; set; }
}

public class MessageData {
	public Message_Head head { get; set; }
	public Message_Body body { get; set; }
}

public class MessageParse {
	
    /// <summary>
    /// ��ȡ�ֽ�������Ϣͷ��Ϣ
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
	public static Message_Head UnParseHead (byte[] buffer) {
		if (buffer.Length >= MessageConst.HEADER_LENGTH) {
			Message_Head head = new Message_Head();
            //��ȡ��־��
            System.Array.Reverse(buffer, 0, 2);
            head.HEADER = System.BitConverter.ToInt16(buffer, 0);
            //��ȡ��Ϣ����
            System.Array.Reverse(buffer, 2, 2);
            head.Length = System.BitConverter.ToInt16(buffer, 2);
            //��ȡУ���
            System.Array.Reverse(buffer, 4, 2);
            head.CHECK_SUM = System.BitConverter.ToInt16(buffer, 4);
            //��ȡЭ���
            System.Array.Reverse(buffer, 6, 2);
            head.ProtoVersion = System.BitConverter.ToInt16(buffer, 6);
            //��ȡTOID
            System.Array.Reverse(buffer, 8, 4);
            head.TOID = System.BitConverter.ToInt32(buffer, 8);
            //��ȡExt1
            System.Array.Reverse(buffer, 12, 4);
            head.EXTID1 = System.BitConverter.ToInt32(buffer, 12);
            //��ȡExt2
            System.Array.Reverse(buffer, 16, 4);
            head.EXTID2 = System.BitConverter.ToInt32(buffer, 16);
			return head;
		}
		return null;
	}
    /// <summary>
    /// ��ȡ�ֽ�������Ϣ����Ϣ
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static MessageData UnParse(byte[] buffer)
    {
        int iHead = MessageConst.HEADER_LENGTH;
        {
            Message_Head head = UnParseHead(buffer);
            if (head != null && head.Length == buffer.Length)
            {
                Message_Body body = new Message_Body();
                System.Array.Reverse(buffer, iHead, 4);
                body.iCommand = System.BitConverter.ToInt32(buffer, iHead);
                body.buffBytes = new byte[head.Length - 4];
                System.Array.Copy(buffer, iHead + 4, body.buffBytes, 0, body.buffBytes.Length);
                MessageData data = new MessageData();
                data.head = head;
                data.body = body;
                return data;
            }
        }
        return null;
    }

    /// <summary>
    /// �����ֽ����ݵ�У����㷨
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static int CalculateCheckSum(byte[] buffer)
    {
        int val1 = 0x77;
        int i = 6;
        int length = buffer.Length;
        while (i < length)
        {
            val1 += System.Convert.ToInt32(buffer[i++]);
        }
        return (val1) & 0x7F7F;
    }

    /// <summary>
    /// ���췢����Ϣͷ
    /// </summary>
    /// <param name="iVersion"></param>
    /// <param name="iByteLength"></param>
    /// <returns></returns>
	public static byte[] ParseHead (int code=0,int toID=0, int extend1=0,int extend2=0){
		byte[] arrBytes = new byte[MessageConst.HEADER_LENGTH];
        //��ʶ��
        System.Array.Copy(System.BitConverter.GetBytes(MessageConst.HEADER), 0, arrBytes, 0, 2);
        System.Array.Reverse(arrBytes, 0, 2);
        //����
        System.Array.Copy(System.BitConverter.GetBytes(0), 0, arrBytes, 2, 2);
        System.Array.Reverse(arrBytes, 2, 2);
        //У���
        System.Array.Copy(System.BitConverter.GetBytes(0), 0, arrBytes, 4, 2);
        System.Array.Reverse(arrBytes, 4, 2);
        //Э���
        System.Array.Copy(System.BitConverter.GetBytes(code), 0, arrBytes, 6, 2);
        System.Array.Reverse(arrBytes, 6, 2);

        if (toID <= 0) toID = MessageConst.ClientId;
        System.Array.Copy(System.BitConverter.GetBytes(toID), 0, arrBytes, 8, 4);
        System.Array.Reverse(arrBytes, 8, 4);
        //��չλ1
		System.Array.Copy(System.BitConverter.GetBytes(extend1),0,arrBytes,12, 4);
        System.Array.Reverse(arrBytes, 12, 4);
        //��չλ2
		System.Array.Copy(System.BitConverter.GetBytes(extend2),0,arrBytes,16, 4);
		System.Array.Reverse(arrBytes, 16, 4);

		return arrBytes;
	}
	/// <summary>
	/// ���췢����Ϣ��
	/// </summary>
	/// <param name="iCommand">Э�������</param>
	/// <param name="protobuf">protobuffer���л��ַ���</param>
	/// <returns>��Ϣ���ֽ�����</returns>
    public static byte[] ParseBody(int iCommand, string protobuf)
    {
        if (!string.IsNullOrEmpty(protobuf))
        {
            byte[] bufBytes = System.Text.Encoding.UTF8.GetBytes(protobuf);
            byte[] arrBytes = new byte[4 + bufBytes.Length];
			System.Array.Copy(System.BitConverter.GetBytes(iCommand),0,arrBytes,0, 4);
			System.Array.Reverse(arrBytes, 0, 4);
            System.Array.Copy(bufBytes, 0, arrBytes, 4, bufBytes.Length);
			return arrBytes;
		}
		return null;
	}
    /// <summary>
    /// ����У��ͺ����ݳ��ȣ����������ֽ�����
    /// </summary>
    /// <param name="iVersion"></param>
    /// <param name="iCommand"></param>
    /// <param name="protobuf"></param>
    /// <returns></returns>
    public static byte[] Parse(int iCommand, string protobuf, int code = 0, int toID = 0, int extend1 = 0, int extend2 = 0)
    {
        byte[] bodyBytes = ParseBody(iCommand, protobuf);
		if (bodyBytes != null) {
			byte[] headBytes = ParseHead(code,toID,extend1,extend2);
            int len = headBytes.Length + bodyBytes.Length;
			byte[] allBytes = new byte[len];
			System.Array.Copy(headBytes,0,allBytes,0, headBytes.Length);
			System.Array.Copy(bodyBytes,0,allBytes,headBytes.Length, bodyBytes.Length);
            int checksum = CalculateCheckSum(allBytes);
            //���³���
            System.Array.Copy(System.BitConverter.GetBytes(len), 0, allBytes, 2, 2);
            System.Array.Reverse(allBytes, 2, 2);
            //����У���
            System.Array.Copy(System.BitConverter.GetBytes(checksum), 0, allBytes, 4, 2);
            System.Array.Reverse(allBytes, 4, 2);

			return allBytes;
		}
		return null;
	}

    /// <summary>
    /// ������Ϣ�ֽ����ݼ����㷨
    /// </summary>
    /// <param name="pkg"></param>
    /// <param name="pkg"></param>
    /// <returns></returns>
    public static void  Encrypt(byte[] pkg, byte[] key)
    {
        for (int i = 0; i < pkg.Length; i++)
        {
            if (i > 0)
            {
                key[i % 8] = (byte)((key[i % 8] + pkg[i - 1]) ^ i);
                pkg[i] = (byte)((pkg[i] ^ key[i % 8]) + pkg[i - 1]);
            }
            else
            {
                pkg[0] = (byte)(pkg[0] ^ key[0]);
            }
        }
    }

    /// <summary>
    /// �ֽ�����ͷ���ֽ��ܻ�ȡ��Ϣ�峤�ȵ���Ϣ
    /// </summary>
    /// <param name="src"></param>
    /// <param name="len"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] src, int len, byte[] key)
    {
        byte[] result = new byte[len];
        //System.Array.Copy(src, 0, result, 0, len);
        for (int i = 0; i < len; i++)
        {
            if (i > 0)
            {
                key[i % 8] = (byte)((key[i % 8] + src[i - 1]) ^ i);
                result[i] = (byte)((src[i] - src[i - 1]) ^ key[i % 8]);
            }
            else
            {
                result[0] = (byte)(src[0] ^ key[0]);
            }
        }
        return result;
    }
    //������ʱ�ֽ��������ڷ���Ԥ��ȡ��Ϣ
    public static byte[] CopyByteArray(byte[] arr)
    {
        int len = arr.Length;
        byte[] result = new byte[len];
        System.Array.Copy(arr, 0, result, 0, len);
        return result;
    }
}
