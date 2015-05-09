﻿// 2015,2014 ,BSD, WinterDev

using System;
using System.Collections.Generic;

using LayoutFarm.Css;
using PixelFarm.Drawing;

namespace LayoutFarm.HtmlBoxes
{

    public class LayoutVisitor : BoxVisitor
    {
        HtmlContainer htmlContainer;
        float totalMarginLeftAndRight;

        Queue<Dictionary<CssBox, PartialBoxStrip>> dicStripPool;
        Queue<List<PartialBoxStrip>> listStripPool;

        Dictionary<CssBox, PartialBoxStrip> readyDicStrip = new Dictionary<CssBox, PartialBoxStrip>();
        List<PartialBoxStrip> readyListStrip = new List<PartialBoxStrip>();

        Stack<CssBox> leftFloatBoxStack = new Stack<CssBox>();
        Stack<CssBox> rightFloatBoxStack = new Stack<CssBox>();
        Stack<CssBox> lineFormattingContextStack = new Stack<CssBox>();

        Stack<float> lineOffsetLeftStack = new Stack<float>();
        Stack<float> lineOffsetRightStack = new Stack<float>();
        Stack<bool> floatingOutOfLineStack = new Stack<bool>();

        CssBox latestLeftFloatBox;
        CssBox latestRightFloatBox;

        static int totalLayoutIdEpisode = 0;
        int episodeId = 1;
        GraphicsPlatform gfxPlatform;
        CssBox latestLineFormattingContextBox;

        public LayoutVisitor(GraphicsPlatform gfxPlatform)
        {
            this.gfxPlatform = gfxPlatform;
        }
        public void Bind(HtmlContainer htmlCont)
        {
            this.htmlContainer = htmlCont;
            if (episodeId == ushort.MaxValue - 1)
            {
                //reset
                totalLayoutIdEpisode = 1;
                episodeId = totalLayoutIdEpisode++;
            }
        }
        public void UnBind()
        {
            this.htmlContainer = null;
            if (dicStripPool != null) dicStripPool.Clear();
            if (listStripPool != null) listStripPool.Clear();

            readyDicStrip.Clear();
            readyListStrip.Clear();
            totalMarginLeftAndRight = 0;

        }
        public GraphicsPlatform GraphicsPlatform
        {
            get { return this.gfxPlatform; }
        }

        internal IFonts SampleIFonts
        {
            get { return this.gfxPlatform.SampleIFonts; }

        }


        protected override void OnPushDifferentContainingBlock(CssBox box)
        {
            this.totalMarginLeftAndRight += (box.ActualMarginLeft + box.ActualMarginRight);
            this.leftFloatBoxStack.Push(this.LatestLeftFloatBox);
            this.rightFloatBoxStack.Push(this.LatestRightFloatBox);
        }
        protected override void OnPopDifferentContaingBlock(CssBox box)
        {
            this.totalMarginLeftAndRight -= (box.ActualMarginLeft + box.ActualMarginRight);
            this.latestLeftFloatBox = this.leftFloatBoxStack.Pop();
            this.latestRightFloatBox = this.rightFloatBoxStack.Pop();
        }

        internal void EnterNewLineFormattingContext(CssBox host)
        {
            //store last box
            lineFormattingContextStack.Push(latestLineFormattingContextBox);
            lineOffsetLeftStack.Push(this.LineOffsetLeft);
            lineOffsetRightStack.Push(this.LineOffsetRight);
            this.leftFloatBoxStack.Push(this.LatestLeftFloatBox);
            this.rightFloatBoxStack.Push(this.LatestRightFloatBox);
            floatingOutOfLineStack.Push(this.FloatingOutOfLineContext);
            
        }
        internal void ExitCurrentLineFormattingContext()
        {
            latestLineFormattingContextBox = lineFormattingContextStack.Pop();
            this.LineOffsetLeft = lineOffsetLeftStack.Pop();
            this.LineOffsetRight = lineOffsetRightStack.Pop();
            this.latestLeftFloatBox = this.leftFloatBoxStack.Pop();
            this.latestRightFloatBox = this.rightFloatBoxStack.Pop();
            this.FloatingOutOfLineContext = this.floatingOutOfLineStack.Pop();
        }
        //----------------------------------------- 
        internal CssBox LatestSiblingBox
        {
            get;
            set;
        }
        internal float LineOffsetLeft
        {
            get;
            set;
        }
        internal float LineOffsetRight
        {
            get;
            set;
        }
        internal bool FloatingOutOfLineContext
        {
            get;
            set;
        }
         
        internal CssBox LatestLeftFloatBox
        {
            get { return this.latestLeftFloatBox; }
            set
            {
                this.latestLeftFloatBox = value;
            }
        }
        internal CssBox LatestRightFloatBox
        {
            get { return this.latestRightFloatBox; }
            set
            {
                this.latestRightFloatBox = value;
            }
        }
        internal bool HasFloatBoxInContext
        {
            get { return this.latestLeftFloatBox != null || this.latestRightFloatBox != null; }
        }
        internal void UpdateRootSize(CssBox box)
        {

            float candidateRootWidth = Math.Max(
                box.CalculateMinimumWidth(this.episodeId) + CalculateWidthMarginTotalUp(box),
                (box.SizeWidth + this.ContainerBlockGlobalX) < CssBoxConstConfig.BOX_MAX_RIGHT ? box.SizeWidth : 0);


            this.htmlContainer.UpdateSizeIfWiderOrHigher(
                this.ContainerBlockGlobalX + candidateRootWidth,
                this.ContainerBlockGlobalY + box.SizeHeight);

        }
        /// <summary>
        /// Get the total margin value (left and right) from the given box to the given end box.<br/>
        /// </summary>
        /// <param name="box">the box to start calculation from.</param>
        /// <returns>the total margin</returns>
        float CalculateWidthMarginTotalUp(CssBox box)
        {

            if ((box.SizeWidth + this.ContainerBlockGlobalX) > CssBoxConstConfig.BOX_MAX_RIGHT ||
                (box.ParentBox != null && (box.ParentBox.SizeWidth + this.ContainerBlockGlobalX) > CssBoxConstConfig.BOX_MAX_RIGHT))
            {
                return (box.ActualMarginLeft + box.ActualMarginRight) + totalMarginLeftAndRight;
            }
            return 0;
        }


        internal void RequestImage(ImageBinder binder, CssBox requestFrom)
        {
            this.htmlContainer.RaiseImageRequest(binder,
                requestFrom,
                false);
        }
        internal void RequestScrollView(CssBox requestFrom)
        {

            this.htmlContainer.RequestScrollView(requestFrom);
        }
        internal float MeasureWhiteSpace(CssBox box)
        {
            //depends on Font of this box           
            float w = this.SampleIFonts.MeasureWhitespace(box.ActualFont);

            if (!(box.WordSpacing.IsEmpty || box.WordSpacing.IsNormalWordSpacing))
            {
                w += CssValueParser.ConvertToPxWithFontAdjust(box.WordSpacing, 0, box);
            }
            return w;
        }
        internal float MeasureStringWidth(char[] buffer, int startIndex, int len, Font f)
        {
            return this.SampleIFonts.MeasureString(buffer, startIndex, len, f).Width;
        }

        //---------------------------------------------------------------
        internal Dictionary<CssBox, PartialBoxStrip> GetReadyStripDic()
        {
            if (readyDicStrip == null)
            {
                if (this.dicStripPool == null || this.dicStripPool.Count == 0)
                {
                    return new Dictionary<CssBox, PartialBoxStrip>();
                }
                else
                {
                    return this.dicStripPool.Dequeue();
                }
            }
            else
            {
                var tmpReadyStripDic = this.readyDicStrip;
                this.readyDicStrip = null;
                return tmpReadyStripDic;
            }
        }
        internal void ReleaseStripDic(Dictionary<CssBox, PartialBoxStrip> dic)
        {
            //clear before add to pool
            dic.Clear();

            if (this.readyDicStrip == null)
            {
                this.readyDicStrip = dic;
            }
            else
            {
                if (this.dicStripPool == null)
                {
                    this.dicStripPool = new Queue<Dictionary<CssBox, PartialBoxStrip>>();
                }


                this.dicStripPool.Enqueue(dic);
            }
        }
        //---------------------------------------------------------------
        internal List<PartialBoxStrip> GetReadyStripList()
        {
            if (readyListStrip == null)
            {
                if (this.dicStripPool == null || this.dicStripPool.Count == 0)
                {
                    return new List<PartialBoxStrip>();
                }
                else
                {
                    return this.listStripPool.Dequeue();
                }
            }
            else
            {
                var tmpReadyListStrip = this.readyListStrip;
                this.readyListStrip = null;
                return tmpReadyListStrip;
            }
        }
        internal void ReleaseStripList(List<PartialBoxStrip> list)
        {
            //clear before add to pool
            list.Clear();

            if (this.readyListStrip == null)
            {
                this.readyListStrip = list;
            }
            else
            {
                if (this.listStripPool == null)
                {
                    this.listStripPool = new Queue<List<PartialBoxStrip>>();
                }
                this.listStripPool.Enqueue(list);
            }
        }


        //--------------------------------------------------------------- 
        internal int EpisodeId
        {
            get { return this.episodeId; }
        }
        //---------------------------------------------------------------

#if DEBUG
        int dbugIndentLevel;
        internal bool dbugEnableLogRecord;
        internal List<string> logRecords = new List<string>();
        public enum PaintVisitorContextName
        {
            Init
        }
        public void dbugResetLogRecords()
        {
            this.dbugIndentLevel = 0;
            logRecords.Clear();
        }
        public void dbugEnterNewContext(CssBox box, PaintVisitorContextName contextName)
        {

            if (this.dbugEnableLogRecord)
            {
                var controller = CssBox.UnsafeGetController(box);
                //if (box.__aa_dbugId == 7)
                //{
                //}
                logRecords.Add(new string('>', dbugIndentLevel) + dbugIndentLevel.ToString() +
                    "x:" + box.Left + ",y:" + box.Top + ",w:" + box.SizeWidth + "h:" + box.SizeHeight +
                    " " + box.ToString() + ",id:" + box.__aa_dbugId);

                dbugIndentLevel++;
            }
        }
        public void dbugExitContext()
        {
            if (this.dbugEnableLogRecord)
            {
                logRecords.Add(new string('<', dbugIndentLevel) + dbugIndentLevel.ToString());
                dbugIndentLevel--;
                if (dbugIndentLevel < 0)
                {
                    throw new NotSupportedException();
                }
            }
        }
#endif
    }


}