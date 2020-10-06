using System;

namespace BetterLanis.UserSettings.ThemeResources
{
    [Serializable]
    class ThemePreset
    {
        #region General
        public string AccentColor { get; set; } = "#3F8DFF";

        public string BackgroundColor { get; set; } = "#222222";
        public string SecondBackgroundColor { get; set; } = "#2B2B2B";

        public string OutlineColor { get; set; } = "#555555";
        public string SeperatorColor { get; set; } = "#666666";
        #endregion

        #region Text
        public string TextColor { get; set; } = "#FFFFFF";
        public string SecondTextColor { get; set; } = "#999999";
        public string WarningTextColor { get; set; } = "#FF4949";
        #endregion

        #region TextBox
        public string TextBoxBackgroundColor { get; set; } = "#1D1D1D";
        public string SelectionColor { get; set; } = "#505050";
        #endregion

        #region Button
        public string ButtonBackgroundColor { get; set; } = "#1D1D1D";
        public string ButtonHoverColor { get; set; } = "#343434";
        public string ButtonTextColor { get; set; } = "#AAAAAA";
        public string ButtonImageColor { get; set; } = "#FFFFFF";
        public string AccentButtonTextColor { get; set; } = "#FFFFFF";
        #endregion

        #region ToolTip
        public string TooltipTextColor { get; set; } = "#FFFFFF";
        public string TooltipBackgroundColor { get; set; } = "#333333";
        #endregion

        #region Scrollbar
        public string ScrollBarBackgroundColor { get; set; } = "#1F1F1F";
        public string ScrollBarThumbColor { get; set; } = "#151515";
        #endregion

        #region ToggleButton
        public string ToggleButtonBackgroundColor { get; set; } = "#3E3E3E";
        public string ToggleButtonThumbColor { get; set; } = "#FFFFFF";
        #endregion

        #region School
        public string SchoolNameColor { get; set; } = "#DEDEDE";
        public string SchoolDistrictColor { get; set; } = "#A6A6A6";
        public string SchoolLocalColor { get; set; } = "#666666";
        #endregion

        #region Logo
        public string LogoBetterColor { get; set; } = "#000000";
        public string LogoLanisColor { get; set; } = "#FFFFFF";
        #endregion
    }
}