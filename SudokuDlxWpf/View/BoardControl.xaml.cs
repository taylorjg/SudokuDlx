﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class BoardControl : IBoardControl
    {
        private const int GridLineThickness = 4;
        private const int GridLineHalfThickness = GridLineThickness / 2;
        private const int GridLineQuarterThickness = GridLineThickness / 4;
        private double _sw;
        private double _sh;

        private class DigitTag
        {
            public FrameworkElement FrameworkElement { get; }
            public Coords Coords { get; }
            public int Value { get; }
            public bool IsInitialValue { get; }

            public DigitTag(FrameworkElement frameworkElement, Coords coords, int value, bool isInitialValue)
            {
                FrameworkElement = frameworkElement;
                Coords = coords;
                Value = value;
                IsInitialValue = isInitialValue;
            }
        }

        public BoardControl()
        {
            InitializeComponent();
        }

        public void Initialise()
        {
            _sw = (ActualWidth - GridLineThickness) / 9;
            _sh = (ActualHeight - GridLineThickness) / 9;
            DrawGrid();
        }

        public void Reset()
        {
            RemoveDigits();
        }

        public void AddInitialValues(IEnumerable<InitialValue> initialValues)
        {
            foreach (var initialValue in initialValues)
                AddDigit(initialValue.Coords, initialValue.Value, true);
        }

        public void AddDigit(Coords coords, int value)
        {
            AddDigit(coords, value, false);
        }

        public void RemoveDigit(Coords coords)
        {
            Canvas.Children
                .OfType<FrameworkElement>()
                .Where(fe => fe.Tag is DigitTag)
                .Where(fe => ((DigitTag)fe.Tag).Coords.Equals(coords))
                .ToList()
                .ForEach(fe => Canvas.Children.Remove(fe));
        }

        private void AddDigit(Coords coords, int value, bool isInitialValue)
        {
            // http://stackoverflow.com/questions/17828417/centering-text-vertically-and-horizontally-in-textblock-and-passwordbox-in-windo

            var textBlock = new TextBlock
            {
                Text = Convert.ToString(value),
                FontSize = 48,
                Foreground = new SolidColorBrush(Colors.Black),
                Opacity = isInitialValue ? 0.6 : 1.0,
                TextAlignment = TextAlignment.Center
            };

            var border = new Border
            {
                Width = _sw,
                Height = _sh,
                Child = textBlock
            };

            var tag = new DigitTag(border, coords, value, isInitialValue);
            border.Tag = tag;

            Canvas.SetLeft(border, coords.Col * _sw + GridLineHalfThickness);
            Canvas.SetTop(border, coords.Row * _sh + GridLineHalfThickness);
            Canvas.Children.Add(border);
        }

        private void DrawGrid()
        {
            foreach (var row in Enumerable.Range(0, 10))
            {
                var isThickLine = row % 3 == 0;
                var full = isThickLine ? GridLineThickness : GridLineHalfThickness;
                var half = isThickLine ? GridLineHalfThickness : GridLineQuarterThickness;
                var line = new Line
                {
                    X1 = 0,
                    Y1 = _sh * row + half,
                    X2 = 9 * _sw + GridLineThickness,
                    Y2 = _sh * row + half,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = full
                };
                Canvas.Children.Add(line);
            }

            foreach (var col in Enumerable.Range(0, 10))
            {
                var isThickLine = col % 3 == 0;
                var full = isThickLine ? GridLineThickness : GridLineHalfThickness;
                var half = isThickLine ? GridLineHalfThickness : GridLineQuarterThickness;
                var line = new Line
                {
                    X1 = _sw * col + half,
                    Y1 = GridLineHalfThickness,
                    X2 = _sw * col + half,
                    Y2 = GridLineHalfThickness + 9 * _sh,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = full
                };
                Canvas.Children.Add(line);
            }
        }

        private void RemoveDigits()
        {
            Canvas.Children
                .OfType<FrameworkElement>()
                .Where(fe => fe.Tag is DigitTag)
                .ToList()
                .ForEach(fe => Canvas.Children.Remove(fe));
        }
    }
}
