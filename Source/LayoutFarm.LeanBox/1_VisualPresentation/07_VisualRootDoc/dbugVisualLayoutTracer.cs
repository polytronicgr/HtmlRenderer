﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace LayoutFarm.Presentation
{

#if DEBUG
    public class dbugVisualLayoutTracer
    {
        FileStream fs;
        StreamWriter strmWriter;
        VisualRoot visualroot;
        string outputFileName = null;
        int msgLineNum = 0;
        Stack<object> elementStack = new Stack<object>();

        int indentCount = 0;
        int myTraceCount = 0;
        static int tracerCount = 0;


        public dbugVisualLayoutTracer(VisualRoot visualroot)
        {
            this.visualroot = visualroot;
            myTraceCount = tracerCount;
            ++tracerCount;
            outputFileName = dbugCoreConst.dbugRootFolder + "\\layout_trace\\" + myTraceCount + "_" + Guid.NewGuid().ToString() + ".txt";
        }
        public override string ToString()
        {
            return msgLineNum.ToString();
        }
        public void BeginNewContext()
        {
            ++indentCount;
        }
        public void EndCurrentContext()
        {
            --indentCount;
        }
        public void PushVisualElement(ArtVisualElement v)
        {
            elementStack.Push(v);
            BeginNewContext();
        }
        public void PopVisualElement()
        {
            elementStack.Pop();
            EndCurrentContext();

        }
        public void PushLayerElement(VisualLayer layer)
        {
            elementStack.Push(layer);
            BeginNewContext();
        }
        public void PopLayerElement()
        {
            elementStack.Pop();
            EndCurrentContext();

        }

        public object PeekElement()
        {
            return elementStack.Peek();
        }

        public void Start()
        {

            fs = new FileStream(outputFileName, FileMode.Create);
            strmWriter = new StreamWriter(fs);
            strmWriter.AutoFlush = true;
        }
        public void Stop()
        {
            strmWriter.Close();
            strmWriter.Dispose();

            fs.Close();
            fs.Dispose();
            fs = null;

        }
        public void WriteInfo(ArtVisualElement v, string info, string indentPrefix, string indentPostfix)
        {
            ++msgLineNum;
            ShouldBreak();
            strmWriter.Write(new string('\t', indentCount));
            strmWriter.Write(indentPrefix + indentCount + indentPostfix + info + " ");
            strmWriter.Write(v.dbug_FullElementDescription());
            strmWriter.Write("\r\n"); strmWriter.Flush();

        }
        public void WriteInfo(string info)
        {
            ++msgLineNum;
            ShouldBreak();
            strmWriter.Write(new string('\t', indentCount));
            strmWriter.Write(info);
            strmWriter.Write("\r\n"); strmWriter.Flush();
        }
        public void WriteInfo(string info, ArtVisualElement v)
        {
            ++msgLineNum;
            ShouldBreak();
            strmWriter.Write(new string('\t', indentCount));
            strmWriter.Write(info);
            strmWriter.Write(v.dbug_FullElementDescription());
            strmWriter.Write("\r\n"); strmWriter.Flush();
        }
        public void WriteInfo(string info, VisualLayer layer)
        {
            ++msgLineNum;
            ShouldBreak();
            strmWriter.Write(new string('\t', indentCount));
            strmWriter.Write(info);
            strmWriter.Write(layer.ToString());
            strmWriter.Write("\r\n"); strmWriter.Flush();
        }
        public void Flush()
        {
            strmWriter.Flush();
        }
        void ShouldBreak()
        {

        }

    }
#endif

}
