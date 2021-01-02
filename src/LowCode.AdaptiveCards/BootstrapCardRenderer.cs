using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Html;
using System;
using System.Collections.Generic;

namespace LowCode.AdaptiveCards
{
    public class BootstrapCardRenderer : AdaptiveCardRendererBase<HtmlTag, AdaptiveRenderContext>
    {
        public BootstrapCardRenderer()
        {
            ElementRenderers.Set<AdaptiveCard>(AdaptiveCardRender);
            ElementRenderers.Set<AdaptiveTextBlock>(TextBlockRender);
            ElementRenderers.Set<AdaptiveTextInput>(TextInputRender);
            ElementRenderers.Set<AdaptiveSubmitAction>(SubmitActionRender);
        }

        protected override AdaptiveSchemaVersion GetSupportedSchemaVersion()
        {
            return new AdaptiveSchemaVersion(1, 3);
        }

        protected static HtmlTag AdaptiveCardRender(AdaptiveCard card, AdaptiveRenderContext context)
        {
            var container = new DivTag()
                .AddClass("container");

            AddContainerElements(container, card.Body, context);
            AddActions(container, card.Actions, context);

            return container;
        }

        private static void AddActions(HtmlTag tag, List<AdaptiveAction> actions, AdaptiveRenderContext context)
        {
            foreach (var action in actions)
            {
                tag.Children.Add(context.Render(action));
            }
        }

        private static void AddContainerElements(HtmlTag tag, List<AdaptiveElement> elements, AdaptiveRenderContext context)
        {
            foreach (var element in elements)
            {
                tag.Children.Add(context.Render(element));
            }
        }

        protected static HtmlTag TextBlockRender(AdaptiveTextBlock text, AdaptiveRenderContext context)
        {
            return new HtmlTag("p")
                .SetInnerText(text.Text);
        }

        protected static HtmlTag TextInputRender(AdaptiveTextInput input, AdaptiveRenderContext context)
        {
            var formGroup = new DivTag()
                .AddClass("form-group");

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
                .Attr("title", input.ErrorMessage);

            if (input.IsRequired)
            {
                formControl.Attr("required", "required");
            }

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
    }
}
