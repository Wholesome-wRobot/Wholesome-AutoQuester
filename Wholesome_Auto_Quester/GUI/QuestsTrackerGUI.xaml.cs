﻿using System;
using System.Linq;
using System.Windows;
using robotManager.Helpful;
using Wholesome_Auto_Quester.Bot;
using Wholesome_Auto_Quester.Database.Models;
using Wholesome_Auto_Quester.Database.Objectives;
using wManager.Wow.ObjectManager;

namespace Wholesome_Auto_Quester.GUI {
    public partial class QuestsTrackerGUI {
        public QuestsTrackerGUI() {
            InitializeComponent();
            DiscordLink.RequestNavigate += (sender, e) => { System.Diagnostics.Process.Start(e.Uri.ToString()); };
            detailsPanel.Visibility = Visibility.Hidden;
        }

        public void AddToBLClicked(object sender, RoutedEventArgs e) {
            if (sourceQuestsList.SelectedItem != null) {
                ModelQuest selected = (ModelQuest) sourceQuestsList.SelectedItem;
                WholesomeAQSettings.AddQuestToBlackList(selected.Id);
                UpdateQuestsList();
            }
        }

        public void RmvFromBLClicked(object sender, RoutedEventArgs e) {
            if (sourceQuestsList.SelectedItem != null) {
                ModelQuest selected = (ModelQuest) sourceQuestsList.SelectedItem;
                WholesomeAQSettings.RemoveQuestFromBlackList(selected.Id);
                UpdateQuestsList();
            }
        }

        public void ShowWindow() {
            Dispatcher.BeginInvoke((Action) (() => { Show(); }));
        }

        public void HideWindow() {
            Dispatcher.BeginInvoke((Action) (() => { Hide(); }));
        }

        public void UpdateQuestsList() {
            Dispatcher.BeginInvoke((Action) (() => {
                object selectedQuest = sourceQuestsList.SelectedItem;
                sourceQuestsList.ItemsSource = null;
                Vector3 myPos = ObjectManager.Me.PositionWithoutType;
                sourceQuestsList.ItemsSource = WAQTasks.Quests
                    .OrderBy(q => q.Status)
                    .ThenBy(q => {
                        if (q.NpcQuestGivers.Count <= 0) return float.PositiveInfinity;
                        return q.NpcQuestGivers.Min(qg =>
                            new Vector3(qg.PositionX, qg.PositionY, qg.PositionZ).DistanceTo(myPos));
                    });

                if (selectedQuest != null && sourceQuestsList.Items.Contains(selectedQuest))
                    sourceQuestsList.SelectedItem = selectedQuest;
                else
                    detailsPanel.Visibility = Visibility.Hidden;

                questTitleTop.Text = $"Quests ({WAQTasks.Quests.Count})";
            }));
        }

        public void UpdateTasksList() {
            Dispatcher.BeginInvoke((Action) (() => {
                object selectedTask = sourceTasksList.SelectedItem;
                sourceTasksList.ItemsSource = null;
                sourceTasksList.ItemsSource = WAQTasks.TasksPile;
                if (selectedTask != null && sourceTasksList.Items.Contains(selectedTask))
                    sourceTasksList.SelectedItem = selectedTask;
                tasksTitleTop.Text = $"Current Tasks ({WAQTasks.TasksPile.Count})";
            }));
        }

        public void SelectQuest(object sender, RoutedEventArgs e) {
            ModelQuest selected = (ModelQuest) sourceQuestsList.SelectedItem;
            if (selected != null) {
                questTitle.Text = $"{selected.LogTitle}";
                questId.Text = $"Entry: {selected.Id}";
                questLevel.Text = $"Level: {selected.QuestLevel}";

                Vector3 myPos = ObjectManager.Me.PositionWithoutType;
                
                // quest givers
                string qg = "";
                selected.NpcQuestGivers.ForEach(q => qg += $"{q.Id}");
                selected.WorldObjectQuestGivers.ForEach(q => qg += $"{q.Entry}");
                questGivers.Text = $"Quest Givers: {qg}";

                // quest turners
                string qt = "";
                selected.NpcQuestTurners.ForEach(q => qt += $"{q.Id}");
                selected.WorldObjectQuestTurners.ForEach(q => qt += $"{q.Entry}");
                questTurners.Text = $"Quest Turners: {qt}";

                // status
                questStatus.Text = $"Status: {selected.Status}";

                // previous quests
                string qp = "";
                selected.PreviousQuestsIds.ForEach(q => qp += q + " ");
                questPrevious.Text = $"Previous quests: {qp}";

                // next quests
                string qn = "";
                selected.NextQuestsIds.ForEach(q => qn += q + " ");
                questNext.Text = $"Next quests: {qn}";

                // exploration objectives
                string explorationsObjectives = "Explore: ";
                foreach (ExplorationObjective obje in selected.ExplorationObjectives)
                    explorationsObjectives += $"\n    [{selected.ExplorationObjectives.IndexOf(obje) + 1}] {obje.Area.GetPosition}";
                explorations.Text = explorationsObjectives;

                // Interact objectives
                string interactString = "Interact: ";
                foreach (InteractObjective obje in selected.InteractObjectives)
                    interactString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.ItemName} ({obje.WorldObjects.Count} found)";
                interactObjectives.Text = interactString;

                // gather objectives
                string gatherObjectsString = "Gather: ";
                foreach (GatherObjective obje in selected.GatherObjectives)
                    gatherObjectsString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.ItemName} ({obje.WorldObjects.Count} found)";
                questGatherObjects.Text = gatherObjectsString;

                // kill objectives
                string creaturesToKillString = "Kill: ";
                foreach (KillObjective obje in selected.KillObjectives)
                    creaturesToKillString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.CreatureName} ({obje.WorldCreatures.Count} found)";
                questKillCreatures.Text = creaturesToKillString;

                // kill&loot objectives
                string creaturesToLootString = "Kill & Loot: ";
                foreach (KillLootObjective obje in selected.KillLootObjectives)
                    creaturesToLootString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.ItemName} on {obje.CreatureName} ({obje.WorldCreatures.Count} found)";
                questLootCreatures.Text = creaturesToLootString;

                // Prerequisite gathers
                string prerequisiteGathersString = "Prerequisite Gathers: ";
                foreach (GatherObjective obje in selected.PrerequisiteGatherItems)
                    prerequisiteGathersString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.ItemName} ({obje.WorldObjects.Count} found)";
                prerequisiteGathers.Text = prerequisiteGathersString;

                // Prerequisite loots
                string prerequisiteLootsString = "Prerequisite Loots: ";
                foreach (KillLootObjective obje in selected.PrerequisiteLootItems)
                    prerequisiteLootsString +=
                        $"\n    [{obje.ObjectiveIndex}] {obje.Amount} x {obje.ItemName} on {obje.CreatureName} ({obje.WorldCreatures.Count} found)";
                prerequisiteLoots.Text = prerequisiteLootsString;

                if (WholesomeAQSettings.CurrentSetting.BlacklistesQuests.Contains(selected.Id)) {
                    ButtonAddToBl.IsEnabled = false;
                    ButtonRmvFromBl.IsEnabled = true;
                } else {
                    ButtonAddToBl.IsEnabled = true;
                    ButtonRmvFromBl.IsEnabled = false;
                }

                detailsPanel.Visibility = Visibility.Visible;
            }
        }
    }
}