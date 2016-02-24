﻿using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewToStateConverter : IConverter<TailViewState>
    {
        private static class Structure
        {
            public const string Root = "TailView";

            public const string FileName = "FileName";
            public const string SelectedFilter = "SelectedSearch";

            public const string SearchList = "SearchList";
            public const string SearchItem = "SearchItem";

            public const string Text = "Text";
            public const string Filter = "Filter";
            public const string UseRegEx = "UseRegEx";
            public const string Highlight = "Highlight";
            public const string Alert = "Alert";
            public const string IgnoreCase = "IgnoreCase";

            public const string Swatch = "Swatch";
            public const string Hue = "Hue";
            public const string Icon = "Icon";
        }

        public TailViewState Convert(State state)
        {
            if (state == null || state == State.Empty)
                return GetDefaultValue();

            var doc = XDocument.Parse(state.Value);

            var root = doc.ElementOrThrow(Structure.Root);
            var filename = root.ElementOrThrow(Structure.FileName);
            var selectedFilter = root.ElementOrThrow(Structure.SelectedFilter);

            var searchStates = root.Element(Structure.SearchList)
                .Elements(Structure.SearchItem)
                .Select((element,index) =>
                {
                    var text = XmlEx.ElementOrThrow((XElement) element, Structure.Text);
                    var position = ParseEx.ParseInt(element.Attribute(Structure.Filter).Value).ValueOr(() => index);
                    var filter = ParseEx.ParseBool(element.Attribute(Structure.Filter).Value).ValueOr(() => true);
                    var useRegEx = ParseEx.ParseBool(element.Attribute(Structure.UseRegEx).Value).ValueOr(() => false);
                    var highlight = ParseEx.ParseBool(element.Attribute(Structure.Highlight).Value).ValueOr(() => true);
                    var alert = ParseEx.ParseBool(element.Attribute(Structure.Alert).Value).ValueOr(() => false);
                    var ignoreCase = ParseEx.ParseBool(element.Attribute(Structure.IgnoreCase).Value).ValueOr(() => true);

                    var swatch = element.Attribute(Structure.Swatch).Value;
                    var hue = element.Attribute(Structure.Hue).Value;
                    var icon = element.Attribute(Structure.Icon).Value;

                    return new SearchState(text, position, useRegEx,highlight,filter,alert,ignoreCase,swatch,icon,hue);
                }).ToArray();
            return new TailViewState(filename, selectedFilter, searchStates);
        }

        public State Convert(TailViewState state)
        {
            if (state == null || state == TailViewState.Empty)
                return State.Empty;

            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.FileName, state.FileName));
            root.Add(new XElement(Structure.SelectedFilter, state.SelectedSearch));

            var list = new XElement(Structure.SearchList);

            var searchItemsArray = state.SearchItems.Select(f => new XElement(Structure.SearchItem,
                new XElement(Structure.Text, f.Text),
                new XAttribute(Structure.Filter, f.Filter),
                new XAttribute(Structure.UseRegEx, f.UseRegEx),
                new XAttribute(Structure.Highlight, f.Highlight),
                new XAttribute(Structure.Alert, f.Alert),
                new XAttribute(Structure.IgnoreCase, f.IgnoreCase),
                new XAttribute(Structure.Swatch, f.Swatch),
                new XAttribute(Structure.Hue, f.Hue),
                new XAttribute(Structure.Icon, f.Icon)));

            searchItemsArray.ForEach(list.Add);

            root.Add(list);

            XDocument doc = new XDocument(root);
            return new State(1, doc.ToString());
        }

        public TailViewState GetDefaultValue()
        {
            return TailViewState.Empty;
        }
    }
}