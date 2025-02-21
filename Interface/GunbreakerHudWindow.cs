﻿using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class GunbreakerHudWindow : HudWindow {
        public override uint JobId => 37;

        private new int BarHeight => 13;
        private new int BarWidth => 254;
        private new int XOffset => 127;
        private new int YOffset => 466;
        
        public GunbreakerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPowderGauge();
            DrawTargetBar();
        }

        private void DrawPowderGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<GNBGauge>();
            var powderColor = 0xFFFEAD43;

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 2)  / 2;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 33;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if(gauge.NumAmmo > 0) drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 
                powderColor, powderColor, powderColor, powderColor
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if(gauge.NumAmmo > 1) drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 
                powderColor, powderColor, powderColor, powderColor
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}