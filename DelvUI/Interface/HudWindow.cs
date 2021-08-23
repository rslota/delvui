using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DelvUI.Interface {
    
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly ClientState ClientState;
        protected readonly GameGui GameGui;
        protected readonly JobGauges JobGauges;
        protected readonly ObjectTable ObjectTable;
        protected readonly PluginConfiguration PluginConfiguration;
        protected readonly TargetManager TargetManager;
        
        private readonly Vector2 _barsize;

        public abstract uint JobId { get; }

        protected float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected int XOffset => 160;
        protected int YOffset => 460;
        protected int BarHeight => 50;
        protected int BarWidth => 270;
        protected Vector2 BarSize => _barsize;
        
        protected HudWindow(
            ClientState clientState, 
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration, 
            TargetManager targetManager 
            ) {
            ClientState = clientState;
            GameGui = gameGui;
            JobGauges = jobGauges;
            ObjectTable = objectTable;
            PluginConfiguration = pluginConfiguration;
            TargetManager = targetManager;
            _barsize = new Vector2(BarWidth, BarHeight);
        }

        protected virtual void DrawHealthBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var actor = ClientState.LocalPlayer;
            var scale = (float) actor?.CurrentHp / actor.MaxHp;

            var cursorPos = new Vector2(CenterX - BarWidth - XOffset, CenterY + YOffset);

            DrawOutlinedText($"{actor.Name.Abbreviate().Truncate(16)}", new Vector2(cursorPos.X + 5, cursorPos.Y -22));
            
            var hp = $"{actor.MaxHp.KiloFormat(),6} | ";
            var hpSize = ImGui.CalcTextSize(hp);
            var percentageSize = ImGui.CalcTextSize("100");
            DrawOutlinedText(hp, new Vector2(cursorPos.X + BarWidth - hpSize.X - percentageSize.X - 5, cursorPos.Y -22));
            DrawOutlinedText($"{(int)(scale * 100),3}", new Vector2(cursorPos.X + BarWidth - percentageSize.X - 5, cursorPos.Y -22));
            
            ImGui.SetCursorPos(cursorPos);
            
            if (ImGui.BeginChild("health_bar", BarSize)) {
                var colors = PluginConfiguration.JobColorMap[ClientState.LocalPlayer.ClassJob.Id];
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(BarWidth * scale, BarHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                if (ImGui.IsItemClicked()) {
                    TargetManager.SetTarget(actor);
                }
                
                ImGui.EndChild();
            }
        }

        protected virtual void DrawPrimaryResourceBar() {
            var actor = ClientState.LocalPlayer;

            if (actor?.CurrentMp != null) {
                var scale = (float) actor.CurrentMp / actor.MaxMp;
                var barSize = new Vector2(254, 13);
                var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 27);
            
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y), 
                    0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }
        
        protected virtual void DrawTargetBar() {
            var target = TargetManager.SoftTarget ?? TargetManager.Target;

            if (!(target is Character actor)) {
                return;
            }

            var scale = (float) (actor.MaxHp > 0.00 ? actor.CurrentHp / actor.MaxHp : 0.00);
            var cursorPos = new Vector2(CenterX + XOffset, CenterY + YOffset);
            ImGui.SetCursorPos(cursorPos);
 
            var colors = DetermineTargetPlateColors(actor);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarWidth * scale, BarHeight), 
                colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            var percentage = $"{(int) (scale * 100),3}";
            var percentageSize = ImGui.CalcTextSize(percentage);
            var maxPercentageSize = ImGui.CalcTextSize("100");
            DrawOutlinedText(percentage, new Vector2(cursorPos.X + 5 + maxPercentageSize.X - percentageSize.X, cursorPos.Y - 22));
            DrawOutlinedText($" | {actor.MaxHp.KiloFormat(),-6}", new Vector2(cursorPos.X + 5 + maxPercentageSize.X, cursorPos.Y - 22));
            
            var name = $"{actor.Name.Abbreviate().Truncate(16)}";
            var nameSize = ImGui.CalcTextSize(name);
            DrawOutlinedText(name, new Vector2(cursorPos.X + BarWidth - nameSize.X - 5, cursorPos.Y - 22));

            DrawTargetOfTargetBar(target.TargetObjectId);
        }
        
        protected virtual void DrawTargetOfTargetBar(uint targetActorId) {
            GameObject target = null;
            
            for (var i = 0; i < 200; i += 2) {
                if (ObjectTable[i].ObjectId == targetActorId) {
                    target = ObjectTable[i];
                }
            }
            
            if (!(target is Character actor)) {
                return;
            }

            const int barWidth = 120;
            const int barHeight = 20;
            var barSize = new Vector2(barWidth, barHeight);

            var name = $"{actor.Name.Abbreviate().Truncate(12)}";
            var textSize = ImGui.CalcTextSize(name);

            var cursorPos = new Vector2(CenterX + XOffset + BarWidth + 2, CenterY + YOffset);
            DrawOutlinedText(name, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y - 22));
            ImGui.SetCursorPos(cursorPos);    
            
            var colors = DetermineTargetPlateColors(actor);
            if (ImGui.BeginChild("target_bar", barSize)) {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["background"]);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float)barWidth * actor.CurrentHp / actor.MaxHp, barHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                
                if (ImGui.IsItemClicked()) {
                    TargetManager.SetTarget(target);
                }
                
                ImGui.EndChild();
            }
        }

        protected Dictionary<string, uint> DetermineTargetPlateColors(Character actor) {
            var colors = PluginConfiguration.NPCColorMap["neutral"];
            
            // Still need to figure out the "orange" state; aggroed but not yet attacked.
            switch (actor.ObjectKind) {
                case ObjectKind.Player:
                    colors = PluginConfiguration.JobColorMap[actor.ClassJob.Id];
                    break;

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    colors = PluginConfiguration.NPCColorMap["hostile"];
                    break;

                case ObjectKind.BattleNpc:
                {
                    if (!IsHostileMemory((BattleNpc)actor)) {
                        colors = PluginConfiguration.NPCColorMap["friendly"];
                    }

                    break;
                }
            }

            return colors;
        }

        protected void DrawOutlinedText(string text, Vector2 pos) {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
        }
        
        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) {
            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y));
            ImGui.TextColored(color, text);
        }
        
        public void Draw() {
            if (!ShouldBeVisible() || ClientState.LocalPlayer == null) {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            
            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | 
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin) {
                return;
            }

            Draw(true);
            
            ImGui.End();
        }
        
        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible() {
            if (IsVisible) {
                return true;
            }
            
            if (PluginConfiguration.HideHud) {
                return false;
            }

            var parameterWidget = (AtkUnitBase*) GameGui.GetAddonByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*) GameGui.GetAddonByName("FadeMiddle", 1);
            
            // Display HUD only if parameter widget is visible and we're not in a fade event
            return ClientState.LocalPlayer == null || parameterWidget == null || fadeMiddleWidget == null || !parameterWidget->IsVisible || fadeMiddleWidget->IsVisible;
        }
        
        unsafe bool IsHostileMemory(BattleNpc npc)
        {
            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1) 
                   && *(byte*)(npc.Address + 0x1980) != 0 
                   && *(byte*)(npc.Address + 0x193C) != 1;
        }
    }
}