﻿using System.ComponentModel;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;

namespace MonoGame.Extended.Gui.Controls
{
    public abstract class GuiControl : IMovable, ISizable
    {
        protected GuiControl()
        {
            Color = Color.White;
            TextColor = Color.White;
            IsEnabled = true;
            IsVisible = true;
            Controls = new GuiControlCollection(this);
            Origin = Vector2.One * 0.5f;
        }

        protected GuiControl(TextureRegion2D backgroundRegion)
            : this()
        {
            BackgroundRegion = backgroundRegion;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        public GuiControl Parent { get; internal set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        public Rectangle BoundingRectangle
        {
            get
            {
                var position = Parent != null ? Parent.Position - Parent.Size * Parent.Origin + Position : Position;
                return new Rectangle((position - Size * Origin).ToPoint(), (Point)Size);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Thickness Margin { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Thickness Padding { get; set; }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsFocused { get; set; }

        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Size2 Size { get; set; }
        public Color Color { get; set; }
        public BitmapFont Font { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Vector2 TextOffset { get; set; }
        public TextureRegion2D BackgroundRegion { get; set; }
        public GuiControlCollection Controls { get; }

        public float Width
        {
            get { return Size.Width; }
            set { Size = new Size2(value, Size.Height); }
        }

        public float Height
        {
            get { return Size.Height; }
            set { Size = new Size2(Size.Width, value); }
        }

        public Size2 DesiredSize { get; protected set; }
        public Size2 ActualSize { get; protected set; }

        public void Measure(Size2 availableSize)
        {
            DesiredSize = CalculateDesiredSize(availableSize);
        }

        protected virtual Size2 CalculateDesiredSize(Size2 availableSize)
        {
            if (!Size.IsEmpty)
                return Size;

            return BackgroundRegion?.Size ?? Size2.Empty;
        }

        public Rectangle ClippingRectangle
        {
            get
            {
                var r = BoundingRectangle;
                return new Rectangle(r.Left + Padding.Left, r.Top + Padding.Top, 
                    r.Width - Padding.Right - Padding.Left, r.Height - Padding.Bottom - Padding.Top);
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                DisabledStyle?.ApplyIf(this, !_isEnabled);
            }
        }

        public bool IsVisible { get; set; }
        public GuiControlStyle HoverStyle { get; set; }

        private GuiControlStyle _disabledStyle;
        public GuiControlStyle DisabledStyle
        {
            get { return _disabledStyle; }
            set
            {
                _disabledStyle = value;
                DisabledStyle?.ApplyIf(this, !_isEnabled);
            }
        }

        public virtual void OnScrolled(int delta) { }

        public virtual void OnKeyTyped(IGuiContext context, KeyboardEventArgs args) { }
        public virtual void OnKeyPressed(IGuiContext context, KeyboardEventArgs args) { }

        public virtual void OnPointerDown(IGuiContext context, GuiPointerEventArgs args) { }
        public virtual void OnPointerUp(IGuiContext context, GuiPointerEventArgs args) { }
        
        public virtual void OnPointerEnter(IGuiContext context, GuiPointerEventArgs args)
        {
            if (IsEnabled)
                HoverStyle?.Apply(this);
        }

        public virtual void OnPointerLeave(IGuiContext context, GuiPointerEventArgs args)
        {
            if (IsEnabled)
                HoverStyle?.Revert(this);
        }

        public void Draw(IGuiContext context, IGuiRenderer renderer, float deltaSeconds)
        {
            DrawBackground(context, renderer, deltaSeconds);
            DrawForeground(context, renderer, deltaSeconds, GetTextInfo(context, Text, BoundingRectangle, HorizontalAlignment.Centre, VerticalAlignment.Centre));
        }

        protected TextInfo GetTextInfo(IGuiContext context, string text, Rectangle targetRectangle, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            var font = Font ?? context.DefaultFont;
            var textRectangle = font.GetStringRectangle(text ?? string.Empty, Vector2.Zero);
            var destinationRectangle = GuiAlignmentHelper.GetDestinationRectangle(horizontalAlignment, verticalAlignment, textRectangle, targetRectangle);
            var textPosition = destinationRectangle.Location.ToVector2();
            var textInfo = new TextInfo(text, font, textPosition, textRectangle.Size.ToVector2(), TextColor, ClippingRectangle);
            return textInfo;
        }

        protected virtual void DrawBackground(IGuiContext context, IGuiRenderer renderer, float deltaSeconds)
        {
            renderer.DrawRegion(BackgroundRegion, BoundingRectangle, Color);
        }

        protected virtual void DrawForeground(IGuiContext context, IGuiRenderer renderer, float deltaSeconds, TextInfo textInfo)
        {
            if (!string.IsNullOrWhiteSpace(textInfo.Text))
                renderer.DrawText(textInfo.Font, textInfo.Text, textInfo.Position + TextOffset, textInfo.Color, textInfo.ClippingRectangle);
        }

        protected struct TextInfo
        {
            public TextInfo(string text, BitmapFont font, Vector2 position, Vector2 size, Color color, Rectangle? clippingRectangle)
            {
                Text = text;
                Font = font;
                Size = size;
                Color = color;
                ClippingRectangle = clippingRectangle;
                Position = position;
            }

            public string Text;
            public BitmapFont Font;
            public Vector2 Size;
            public Color Color;
            public Rectangle? ClippingRectangle;
            public Vector2 Position;
        }
    }
}