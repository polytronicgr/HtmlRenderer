﻿// 2015,2014 ,BSD, WinterDev 
//ArthurHub

using System;
using System.Collections.Generic;
//
using PixelFarm.Drawing;
using LayoutFarm.WebDom;
using LayoutFarm.HtmlBoxes;
using LayoutFarm.UI;
using LayoutFarm.Composers;

namespace LayoutFarm.WebDom
{

    public partial class HtmlElement : DomElement
    {
        CssBox principalBox;
        Css.BoxSpec boxSpec;
        CssRuleSet elementRuleSet;

        internal HtmlElement(HtmlDocument owner, int prefix, int localNameIndex)
            : base(owner, prefix, localNameIndex)
        {
            this.boxSpec = new Css.BoxSpec();
        }
        public Css.BoxSpec Spec
        {
            get { return this.boxSpec; }
        }
        public WellKnownDomNodeName WellknownElementName { get; set; }
        public bool TryGetAttribute(WellknownName wellknownHtmlName, out DomAttribute result)
        {
            var found = base.FindAttribute((int)wellknownHtmlName);
            if (found != null)
            {
                result = found;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
        public bool TryGetAttribute(WellknownName wellknownHtmlName, out string value)
        {
            DomAttribute found;
            if (this.TryGetAttribute(wellknownHtmlName, out found))
            {
                value = found.Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }

        }

        public override void SetAttribute(DomAttribute attr)
        {
            base.SetAttribute(attr);

            switch ((WellknownName)attr.LocalNameIndex)
            {
                case WellknownName.Style:
                    {
                        //parse and evaluate style here


                    } break;
            }
        }
        //------------------------------------
        public CssBox GetPrincipalBox()
        {
            return this.principalBox;
        }
        public void SetPrincipalBox(CssBox box)
        {
            this.principalBox = box;
            this.SkipPrincipalBoxEvalulation = true;
        }
        public override void ClearAllElements()
        {
            //clear presentation 
            if (principalBox != null)
            {
                principalBox.Clear();
            }
            base.ClearAllElements();

        }
        internal bool SkipPrincipalBoxEvalulation
        {
            get;
            set;

        }
        internal static CssBox InternalGetPrincipalBox(HtmlElement element)
        {
            return element.principalBox;
        }
        protected override void OnContentUpdate()
        {
            base.OnContentUpdate();
            OnChangeInIdleState(ElementChangeKind.ContentUpdate);
        }
        //------------------------------------
        protected override void OnChangeInIdleState(ElementChangeKind changeKind)
        {
            //1. 
            this.OwnerDocument.SetDocumentState(DocumentState.ChangedAfterIdle);
            //2.
            this.SkipPrincipalBoxEvalulation = false;
            var cnode = this.ParentNode;
            while (cnode != null)
            {
                ((HtmlElement)cnode).SkipPrincipalBoxEvalulation = false;
                cnode = cnode.ParentNode;
            }
        }

        internal static void InvokeNotifyChangeOnIdleState(HtmlElement elem, ElementChangeKind changeKind)
        {
            elem.OnChangeInIdleState(changeKind);
        }

        internal CssRuleSet ElementRuleSet
        {
            get
            {
                return this.elementRuleSet;
            }
            set
            {
                this.elementRuleSet = value;
            }
        }
        internal bool IsStyleEvaluated
        {
            get;
            set;
        }

    }

    sealed class HtmlRootElement : HtmlElement
    {
        public HtmlRootElement(HtmlDocument ownerDoc)
            : base(ownerDoc, 0, 0)
        {
        }
    }

    sealed class ExternalHtmlElement : HtmlElement
    {
        LazyCssBoxCreator lazyCreator;
        RenderElementWrapperCssBox wrapper;
        public ExternalHtmlElement(HtmlDocument owner, int prefix, int localNameIndex, LazyCssBoxCreator lazyCreator)
            : base(owner, prefix, localNameIndex)
        {
            this.lazyCreator = lazyCreator;
        }
        public CssBox GetCssBox(RootGraphic rootgfx)
        {
            if (wrapper != null) return wrapper;
            RenderElement re;
            object controller;
            lazyCreator(rootgfx, out re, out controller);
            return wrapper = new RenderElementWrapperCssBox(controller, this.Spec, re);
        }

    }
    public delegate void LazyCssBoxCreator(RootGraphic rootgfx, out RenderElement re, out object controller);
    public class HtmlTextNode : DomTextNode
    {
        //---------------------------------
        //this node may be simple text node  
        bool freeze;
        bool hasSomeChar;
        List<CssRun> runs;

        public HtmlTextNode(WebDocument ownerDoc, char[] buffer)
            : base(ownerDoc, buffer)
        {
        }
        public bool IsWhiteSpace
        {
            get
            {
                return !this.hasSomeChar;
            }
        }
        internal void SetSplitParts(List<CssRun> runs, bool hasSomeChar)
        {

            this.freeze = false;
            this.runs = runs;
            this.hasSomeChar = hasSomeChar;
        }
        public bool IsFreeze
        {
            get { return this.freeze; }
        }
#if DEBUG
        public override string ToString()
        {
            return new string(base.GetOriginalBuffer());
        }
#endif

        internal List<CssRun> InternalGetRuns()
        {
            this.freeze = true;
            return this.runs;
        }

    }

    public enum TextSplitPartKind : byte
    {
        Text = 1,
        Whitespace,
        SingleWhitespace,
        LineBreak,
    }


    static class HtmlPredefineNames
    {

        static readonly ValueMap<WellknownName> _wellKnownHtmlNameMap =
            new ValueMap<WellknownName>();

        static UniqueStringTable htmlUniqueStringTableTemplate = new UniqueStringTable();

        static HtmlPredefineNames()
        {
            int j = _wellKnownHtmlNameMap.Count;
            for (int i = 0; i < j; ++i)
            {
                htmlUniqueStringTableTemplate.AddStringIfNotExist(_wellKnownHtmlNameMap.GetStringFromValue((WellknownName)(i + 1)));
            }
        }
        public static UniqueStringTable CreateUniqueStringTableClone()
        {
            return htmlUniqueStringTableTemplate.Clone();
        }

    }

}