using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        public override uint JobId => 27;

        private new static int BarHeight => 20;
        private static int SmallBarHeight => 10;
        private new static int BarWidth => 254;
        private new static int XOffset => 127;
        private new static int YOffset => 466;

        public SummonerHudWindow(
            ClientState clientState, 
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration, 
            TargetManager targetManager
        ) : base(
            clientState,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            targetManager
        ) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawRuinBar();
            DrawActiveDots();
            DrawAetherBar();
            DrawTargetBar();
        }

        private void DrawActiveDots()
        {
            var target = TargetManager.SoftTarget ?? TargetManager.Target;

            if (target is not BattleChara actor)
            {
                return;
            }

            var expiryColor = 0xFF2E2EC7;
            var xPadding = 2;
            var barWidth = (BarWidth / 2) - 1;
            var miasma = actor.StatusList.FirstOrDefault(o => o.StatusId == 1215 || o.StatusId == 180);
            var bio = actor.StatusList.FirstOrDefault(o => o.StatusId == 1214 || o.StatusId == 179 || o.StatusId == 189);

            var miasmaDuration = miasma.RemainingTime;
            var bioDuration = bio.RemainingTime;

            var miasmaColor = miasmaDuration > 5 ? 0xFFFAFFA4 : expiryColor;
            var bioColor = bioDuration > 5 ? 0xFF005239 : expiryColor;

            var xOffset = CenterX - 127;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 46);
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + barWidth - (barSize.X / 30) * miasmaDuration, CenterY + YOffset - 46);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(dotStart, cursorPos + new Vector2(barSize.X, barSize.Y), miasmaColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 30) * bioDuration, barSize.Y), bioColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }
        private void DrawAetherBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var aetherFlowBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            var xPadding = 2;
            var barWidth = (BarWidth / 2) - 1;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 22);
            var barSize = new Vector2(barWidth, BarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 22);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;

            }

        }
        private void DrawRuinBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var ruinBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1212);
            var ruinStacks = ruinBuff.StackCount;

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 34;
            var cursorPos = new Vector2(xPos, yPos);
            var barColor = 0xFFFFFF00;

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (ruinStacks > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), barColor);
                }
                else
                {

                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }

        }
        private void DrawTranceBar()
        {
            //Need to figure this out, trances dont give a visible buff, Api has a buff listing as 808 but unsure if this is correct

        }
        private void DrawEgiAssaultsBar()
        {
            // Does dalamud give ability tracking? This may still need to be a action bar the user creates.
        }
    }
}