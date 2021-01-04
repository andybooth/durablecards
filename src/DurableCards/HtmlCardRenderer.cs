using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Html;
using System;
using System.Collections.Generic;

namespace DurableCards
{
    public class HtmlCardRenderer : AdaptiveCardRendererBase<HtmlTag, AdaptiveRenderContext>
    {
        public HtmlCardRenderer()
        {
            ElementRenderers.Set<AdaptiveCard>(AdaptiveCardRender);
            ElementRenderers.Set<AdaptiveTextBlock>(TextBlockRender);
            ElementRenderers.Set<AdaptiveTextInput>(TextInputRender);
            ElementRenderers.Set<AdaptiveSubmitAction>(SubmitActionRender);
            ElementRenderers.Set<AdaptiveOpenUrlAction>(OpenUrlActionRender);
            ElementRenderers.Set<AdaptiveColumnSet>(ColumnSetRender);
            ElementRenderers.Set<AdaptiveColumn>(ColumnRender);
            ElementRenderers.Set<AdaptiveContainer>(ContainerRender);
            ElementRenderers.Set<AdaptiveFactSet>(FactSetRender);
        }

        protected override AdaptiveSchemaVersion GetSupportedSchemaVersion()
        {
            return new AdaptiveSchemaVersion(1, 3);
        }

        public RenderedAdaptiveCard RenderCard(AdaptiveCard card)
        {
            try
            {
                var context = new AdaptiveRenderContext(HostConfig, ElementRenderers);
                var tag = context.Render(card);
                return new RenderedAdaptiveCard(tag, card, context.Warnings);
            }
            catch (Exception exception)
            {
                throw new AdaptiveRenderException("Failed to render card", exception)
                {
                    CardFallbackText = card.FallbackText
                };
            }
        }

        protected static HtmlTag AdaptiveCardRender(AdaptiveCard card, AdaptiveRenderContext context)
        {
            var html = new HtmlTag("html");
            var head = new HtmlTag("head");
            var body = new HtmlTag("body");
            var form = new HtmlTag("form")
                .Attr("method", "post");

            var stylesheet = new HtmlTag("link")
                .Attr("rel", "stylesheet")
                .Attr("href", "https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.min.css");
            
            var container = new DivTag()
                .AddClass("container");

            html.Append(head);
            html.Append(body);

            head.Append(stylesheet);

            body.Append(form);

            form.Append(container);

            AddContainerElements(container, card.Body, context);
            AddActions(container, card.Actions, context);

            return html;
        }

        private static void AddActions(HtmlTag tag, List<AdaptiveAction> actions, AdaptiveRenderContext context)
        {
            foreach (var action in actions)
            {
                tag.Children.Add(context.Render(action));
            }
        }

        private static void AddContainerElements<T>(HtmlTag tag, List<T> elements, AdaptiveRenderContext context) where T : AdaptiveElement
        {
            foreach (var element in elements)
            {
                tag.Children.Add(context.Render(element));
            }
        }

        protected static HtmlTag TextBlockRender(AdaptiveTextBlock text, AdaptiveRenderContext context)
        {
            return new HtmlTag("p")
                .AddOptionalClass(ConvertSizeToClassName(text.Size))
                .AddOptionalClass(ConvertSizeToClassName(text.Size))
                .Attr("hidden", !text.IsVisible)
                .SetInnerText(text.Text);
        }

        protected static string ConvertSizeToClassName(AdaptiveTextSize textSize)
        {
            switch (textSize)
            {
                case AdaptiveTextSize.ExtraLarge:
                    return "display-1";
                case AdaptiveTextSize.Large:
                    return "display-2";
                case AdaptiveTextSize.Medium:
                    return "display-3";
                case AdaptiveTextSize.Small:
                    return "small";
                default:
                    return null;
            }
        }

        protected static string ConvertWeightToClassName(AdaptiveTextWeight textWeight)
        {
            switch (textWeight)
            {
                case AdaptiveTextWeight.Bolder:
                    return "font-weight-bold";
                case AdaptiveTextWeight.Lighter:
                    return "font-weight-light";
                default:
                    return null;
            }
        }

        protected static HtmlTag TextInputRender(AdaptiveTextInput input, AdaptiveRenderContext context)
        {
            var formGroup = new DivTag()
                .AddClass("form-group")
                .Attr("hidden", !input.IsVisible);

            if (!string.IsNullOrEmpty(input.Label))
            {
                var formLabel = new HtmlTag("label")
                    .Attr("for", input.Id)
                    .SetInnerText(input.Label);

                formGroup.Children.Add(formLabel);
            }

            var formControl = new HtmlTag(input.IsMultiline ? "textarea" : "input")
                .AddClass("form-control")
                .Attr("name", input.Id)
                .Attr("placeholder", input.Placeholder)
                .Attr("pattern", input.Regex)
                .Attr("type", input.Style.ToString())
                .Attr("value", input.Value)
                .Attr("required", input.IsRequired)
                .Attr("title", input.ErrorMessage);

            if (input.MaxLength > 0)
            {
                formControl.Attr("maxlength", input.MaxLength.ToString());
            }

            formGroup.Children.Add(formControl);

            return formGroup;
        }

        protected static HtmlTag SubmitActionRender(AdaptiveSubmitAction action, AdaptiveRenderContext context)
        {
            return new HtmlTag("button")
                .AddClass("btn")
                .AddClass("btn-primary")
                .Attr("type", "submit")
                .SetInnerText(action.Title);
        }

        protected static HtmlTag OpenUrlActionRender(AdaptiveOpenUrlAction action, AdaptiveRenderContext context)
        {
            return new HtmlTag("a")
                .AddClass("btn")
                .AddClass("btn-secondary")
                .Attr("target", "_blank")
                .Attr("href", action.Url?.ToString())
                .SetInnerText(action.Title);
        }

        private HtmlTag ColumnSetRender(AdaptiveColumnSet columnSet, AdaptiveRenderContext context)
        {
            var container = new DivTag()
                .AddClass("container")
                .Attr("hidden", !columnSet.IsVisible);

            var row = new DivTag()
                .AddClass("row");

            container.Append(row);

            AddContainerElements(row, columnSet.Columns, context);

            return container;
        }

        private HtmlTag ColumnRender(AdaptiveColumn column, AdaptiveRenderContext context)
        {
            var div = new DivTag()
                .AddClass("col")
                .Attr("hidden", !column.IsVisible);

            AddContainerElements(div, column.Items, context);

            return div;
        }

        private HtmlTag ContainerRender(AdaptiveContainer container, AdaptiveRenderContext context)
        {
            var div = new DivTag()
                .Attr("hidden", !container.IsVisible);

            AddContainerElements(div, container.Items, context);

            return div;
        }

        private HtmlTag FactSetRender(AdaptiveFactSet factSet, AdaptiveRenderContext context)
        {
            var definitionList = new HtmlTag("dl")
                .AddClass("row")
                .Attr("hidden", !factSet.IsVisible);

            foreach (var fact in factSet.Facts)
            {
                var dt = new HtmlTag("dt")
                    .AddClass("col-sm-3")
                    .SetInnerText(fact.Title);

                var dd = new HtmlTag("dd")
                    .AddClass("col-sm-9")
                    .SetInnerText(fact.Value);

                definitionList.Append(dt);
                definitionList.Append(dd);
            }

            return definitionList;
        }
    }
}

public static class HtmlTagExtensions
{
    public static HtmlTag Attr(this HtmlTag htmlTag, string name, bool enabled)
    {
        if (enabled)
        {
            htmlTag.Attr(name, name);
        }

        return htmlTag;
    }

    public static HtmlTag AddOptionalClass(this HtmlTag htmlTag, string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            return htmlTag;
        }

        return htmlTag.AddClass(className);
    }
}