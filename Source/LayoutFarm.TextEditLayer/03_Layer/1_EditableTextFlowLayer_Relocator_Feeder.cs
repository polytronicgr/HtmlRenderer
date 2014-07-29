﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;


namespace LayoutFarm.Presentation.Text
{


    partial class EditableTextFlowLayer
    {
        class FlowReLocator
        {

            EditableTextFlowLayer flowLayer;
            List<EditableVisualElementLine> sourceLines;
            bool isMultiLine = false;
            int currentRelocatorLineId = 0;
            EditableVisualElementLine currentLine;
            FlowFeeder feeder = null;
            bool isFirstRunOfLine = true;


            private FlowReLocator()
            {

            }
            public int OwnerElementWidth
            {
                get
                {
                    return flowLayer.ownerVisualElement.Width;
                }
            }
            public int OwnerElementHeight
            {
                get
                {
                    return flowLayer.ownerVisualElement.Height;
                }
            }
            public void Load(EditableTextFlowLayer flowLayer)
            {
                this.flowLayer = flowLayer;
                this.feeder = FlowFeeder.GetNewFlowFeeder();
                this.feeder.Load(flowLayer);
                if ((flowLayer.layerFlags & FLOWLAYER_HAS_MULTILINE) != 0)
                {
                    Load((List<EditableVisualElementLine>)flowLayer.lineCollection);
                }
                else
                {
                    Load((EditableVisualElementLine)flowLayer.lineCollection);
                }
            }
            void Load(EditableVisualElementLine sourceLine)
            {
                this.currentLine = sourceLine;
                isFirstRunOfLine = true;
            }
            void Load(List<EditableVisualElementLine> sourceLines)
            {

                if (sourceLines.Count > 0)
                {
                    this.currentLine = sourceLines[0];
                }

                this.sourceLines = sourceLines;
                isMultiLine = true;
                isFirstRunOfLine = true;

            }
            public bool ReadNextRun()
            {
                return feeder.Read2();
            }
            public void Accept()
            {
                ArtEditableVisualTextRun v = null;
                int sourceLineId = feeder.CurrentLineId;

                if (this.currentRelocatorLineId == sourceLineId)
                {

                }
                else if (sourceLineId > this.currentRelocatorLineId)
                {
                    v = feeder.UnsafeRemoveCurrent();
                    currentLine.UnsafeAddLast(v);

                    if (feeder.IsUnStableBlankLine)
                    {
                        feeder.RemoveCurrentBlankLine();
                    }
                }
                else
                {
                    v = feeder.CurrentRun;
                    EditableVisualElementLine sourceLine = v.OwnerEditableLine;
                    sourceLine.SplitToNewLine(v);
                    feeder.Read(); currentRelocatorLineId = sourceLineId + 1;
                    currentLine = sourceLines[currentRelocatorLineId];

                }
                isFirstRunOfLine = false;

            }


            public int FeederState
            {
                get
                {
                    return feeder.ReadState;
                }
            }

            public ArtEditableVisualTextRun CurrentRun
            {
                get
                {
                    return feeder.CurrentRun;
                }

            }
            public void SetCurrentLineTop(int y)
            {
                currentLine.SetTop(y);
            }


            public int CurrentLineId
            {
                get
                {
                    return currentRelocatorLineId;
                }
            }


            public void RemoveCurrentBlankLine()
            {
                feeder.RemoveCurrentBlankLine();

            }
            public bool IsFirstRunOfLine
            {
                get
                {
                    return isFirstRunOfLine;
                }
            }
            public void SplitIntoNewLine()
            {

                ArtEditableVisualTextRun currentRun = feeder.CurrentRun;
                EditableVisualElementLine line = currentRun.OwnerEditableLine;
                line.SplitToNewLine(currentRun);

                feeder.Read();
            }

            public void CloseCurrentLineWithLineBreak(int lineWidth, int lineHeight)
            {
                currentLine.EndWithLineBreak = true;
                CloseCurrentLine(lineWidth, lineHeight);
            }
            public void SetCurrentLineSize(int lineWidth, int lineHeight)
            {
                currentLine.SetPostArrangeLineSize(lineWidth, lineHeight);
            }
            public void CloseCurrentLine(int lineWidth, int lineHeight)
            {

                currentLine.SetPostArrangeLineSize(lineWidth, lineHeight);
                if (isMultiLine)
                {
                    if (currentRelocatorLineId < sourceLines.Count - 1)
                    {
                        ++currentRelocatorLineId;
                        currentLine = sourceLines[currentRelocatorLineId];
                    }
                    else
                    {
                        currentRelocatorLineId++;
                        EditableVisualElementLine newLine = new EditableVisualElementLine(flowLayer);
                        flowLayer.AppendLine(newLine);
                        currentLine = newLine;
                    }
                }
                else
                {

                    currentRelocatorLineId++;
                    EditableVisualElementLine newLine = new EditableVisualElementLine(flowLayer);
                    flowLayer.AppendLine(newLine);
                    sourceLines = (List<EditableVisualElementLine>)flowLayer.lineCollection;
                    currentLine = newLine;
                    isMultiLine = true;
                }

                isFirstRunOfLine = true;
            }
            static Stack<FlowReLocator> flowRelcatorStack = new Stack<FlowReLocator>();
            public static FlowReLocator GetNewFlowRelocator()
            {
                if (flowRelcatorStack.Count > 0)
                {
                    return flowRelcatorStack.Pop();
                }
                else
                {
                    return new FlowReLocator();
                }
            }
            public static void FreeFlowRelocator(FlowReLocator flowRelocator)
            {
                flowRelocator.sourceLines = null;
                flowRelocator.isMultiLine = false;
                flowRelocator.currentRelocatorLineId = 0;
                flowRelocator.currentLine = null;
                flowRelocator.isFirstRunOfLine = true;

                FlowFeeder.FreeFlowFeeder(flowRelocator.feeder);
                flowRelocator.feeder = null;

                flowRelcatorStack.Push(flowRelocator);
            }

        }







































        class FlowFeeder
        {

            List<EditableVisualElementLine> sourceLines;
            bool isMultiLine = false;
            int currentFeederLineId = 0;
            EditableVisualElementLine currentLine;

            LinkedListNode<ArtEditableVisualTextRun> curNode;

            int readState = 0;
            EditableTextFlowLayer flowLayer;

            private FlowFeeder()
            {

            }
            public void Load(EditableTextFlowLayer flowLayer)
            {
                this.flowLayer = flowLayer;
                if ((flowLayer.layerFlags & FLOWLAYER_HAS_MULTILINE) != 0)
                {
                    Load((List<EditableVisualElementLine>)flowLayer.lineCollection);
                }
                else
                {
                    Load((EditableVisualElementLine)flowLayer.lineCollection);
                }
            }
            void Load(EditableVisualElementLine sourceLine)
            {

                this.currentLine = sourceLine;
                readState = 2;

            }
            void Load(List<EditableVisualElementLine> sourceLines)
            {

                if (sourceLines.Count > 0)
                {
                    this.currentLine = sourceLines[0];
                }


                readState = 2;
                this.sourceLines = sourceLines;
                isMultiLine = true;
            }




            public int CurrentLineId
            {
                get
                {
                    return currentFeederLineId;
                }
            }
            public void RemoveCurrentBlankLine()
            {
                if (isMultiLine)
                {
                    sourceLines.RemoveAt(currentFeederLineId);
                    if (currentFeederLineId > 0)
                    {
                        currentFeederLineId--;
                        currentLine = sourceLines[currentFeederLineId];
                        curNode = currentLine.Last;
                        if (curNode == null)
                        {
                            if (currentLine.EndWithLineBreak)
                            {
                                readState = 1;
                            }
                            else
                            {
                                readState = 3;
                            }
                        }
                        else
                        {
                            readState = 0;
                        }
                    }
                    else if (currentFeederLineId == 0)
                    {
                        if (sourceLines.Count > 0)
                        {
                            currentLine = sourceLines[currentFeederLineId];
#if DEBUG
                            string ss = currentLine.ToString();
#endif
                            curNode = currentLine.First; ;
                            readState = 2;

                        }
                    }
                }
            }
            public bool IsUnStableBlankLine
            {
                get
                {
                    return currentLine != null && currentLine.Count == 0 && !currentLine.EndWithLineBreak;
                }
            }
            public ArtEditableVisualTextRun UnsafeRemoveCurrent()
            {
                LinkedListNode<ArtEditableVisualTextRun> tobeRemoveNode = curNode;
                ArtEditableVisualTextRun v = tobeRemoveNode.Value;
                EditableVisualElementLine line = v.OwnerEditableLine;

                if (tobeRemoveNode == line.First)
                {
                    curNode = null;
                    readState = 2;
                }
                else
                {
                    curNode = tobeRemoveNode.Previous;
                }
                line.UnsafeRemoveVisualElement(v);
                return v;
            }

            public bool Read2()
            {
                switch (readState)
                {
                    case 2:

                        if (this.currentLine != null)
                        {
                            curNode = currentLine.First; if (curNode != null)
                            {
                                readState = 0; return true;
                            }
                            else if (currentLine.EndWithLineBreak)
                            {
                                readState = 1; return true;
                            }
                        }

                        readState = 4;





                        return true;
                    case 0:
                        {
                            curNode = curNode.Next;
                            if (curNode != null)
                            {
                                return true;
                            }
                            else
                            {
                                if (currentLine.EndWithLineBreak)
                                {
                                    readState = 1;
                                    return true;
                                }
                                else
                                {
                                    if (!isMultiLine)
                                    {
                                        if (flowLayer.lineCollection is List<EditableVisualElementLine>)
                                        {
                                            sourceLines = (List<EditableVisualElementLine>)flowLayer.lineCollection;
                                            isMultiLine = true;
                                        }
                                    }

                                    if (isMultiLine)
                                    {

                                        readState = 3;
                                        if (currentFeederLineId < sourceLines.Count - 1)
                                        {
                                            ++currentFeederLineId; currentLine = sourceLines[currentFeederLineId];
                                            curNode = currentLine.First;
                                            if (curNode != null)
                                            {
                                                readState = 0;
                                                return true;
                                            }
                                            else if (currentLine.EndWithLineBreak)
                                            {
                                                readState = 1;
                                                return true;
                                            }
                                            else
                                            {
                                                readState = 4;
                                                return true;
                                            }
                                        }
                                        return false;
                                    }
                                }
                            }

                        } break;
                    case 1:
                        {
                            if (isMultiLine)
                            {
                                readState = 3;
                                if (currentFeederLineId < sourceLines.Count - 1)
                                {
                                    ++currentFeederLineId; currentLine = sourceLines[currentFeederLineId];
                                    curNode = currentLine.First;
                                    if (curNode != null)
                                    {
                                        readState = 0;
                                        return true;
                                    }
                                    else if (currentLine.EndWithLineBreak)
                                    {
                                        readState = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        readState = 4;
                                        return true;
                                    }
                                }
                                return false;
                            }

                        } break;
                    case 3:
                        {
                            return false;
                        }
                    case 4:
                        {
                            if (isMultiLine)
                            {
                                readState = 3;
                                if (currentFeederLineId < sourceLines.Count - 1)
                                {
                                    ++currentFeederLineId; currentLine = sourceLines[currentFeederLineId];
                                    curNode = currentLine.First;
                                    if (curNode != null)
                                    {
                                        readState = 0;
                                        return true;
                                    }
                                    else if (currentLine.EndWithLineBreak)
                                    {
                                        readState = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        readState = 4;
                                        return true;
                                    }
                                }
                                return false;
                            }
                        } break;
                }
                return false;
            }
            public bool Read()
            {

                switch (readState)
                {
                    case 2:
                        if (this.currentLine != null)
                        {
                            curNode = currentLine.First; if (curNode != null)
                            {
                                readState = 0; return true;
                            }
                            else if (currentLine.EndWithLineBreak)
                            {
                                readState = 1; return true;
                            }
                        }
                        readState = 3; return false;
                    case 0:
                        {
                            curNode = curNode.Next;
                            if (curNode != null)
                            {
                                return true;
                            }
                            else
                            {
                                if (currentLine.EndWithLineBreak)
                                {
                                    readState = 1;
                                    return true;
                                }
                                else
                                {
                                    if (!isMultiLine)
                                    {
                                        if (flowLayer.lineCollection is List<EditableVisualElementLine>)
                                        {
                                            sourceLines = (List<EditableVisualElementLine>)flowLayer.lineCollection;
                                            isMultiLine = true;
                                        }
                                    }

                                    if (isMultiLine)
                                    {

                                        readState = 3;
                                        while (currentFeederLineId < sourceLines.Count - 1)
                                        {
                                            ++currentFeederLineId; currentLine = sourceLines[currentFeederLineId];
                                            curNode = currentLine.First;
                                            if (curNode != null)
                                            {

                                                readState = 0;
                                                return true;
                                            }
                                            else if (currentLine.EndWithLineBreak)
                                            {
                                                readState = 1;
                                                return true;
                                            }
                                        }
                                        return false;
                                    }
                                }
                            }

                        } break;
                    case 1:
                        {
                            if (isMultiLine)
                            {

                                readState = 3;
                                while (currentFeederLineId < sourceLines.Count - 1)
                                {
                                    ++currentFeederLineId; currentLine = sourceLines[currentFeederLineId];
                                    curNode = currentLine.First;
                                    if (curNode != null)
                                    {

                                        readState = 0;
                                        return true;
                                    }
                                    else if (currentLine.EndWithLineBreak)
                                    {
                                        readState = 1;
                                        return true;
                                    }
                                }
                                return false;
                            }

                        } break;
                    case 3:
                        {
                            return false;
                        }
                }
                return false;
            }
            public ArtEditableVisualTextRun CurrentRun
            {
                get
                {
                    if (readState == 0)
                    {
                        return curNode.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public int ReadState
            {
                get
                {
                    return readState;
                }
            }


            static Stack<FlowFeeder> flowFeederStack = new Stack<FlowFeeder>();
            public static FlowFeeder GetNewFlowFeeder()
            {
                if (flowFeederStack.Count > 0)
                {
                    return flowFeederStack.Pop();
                }
                else
                {
                    return new FlowFeeder();
                }
            }
            public static void FreeFlowFeeder(FlowFeeder flowFeeder)
            {
                flowFeeder.sourceLines = null;
                flowFeeder.isMultiLine = false;
                flowFeeder.currentFeederLineId = 0;
                flowFeeder.currentLine = null;
                flowFeeder.curNode = null;
                flowFeeder.readState = 0;
                flowFeederStack.Push(flowFeeder);
            }


        }
    }
}