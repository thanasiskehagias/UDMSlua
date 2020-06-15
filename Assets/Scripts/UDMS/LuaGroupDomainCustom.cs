﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


namespace LuaScripting
{
    public partial class LuaGroupDomain : LuaDomain
    {
        private bool _distTableUpdated;
        private float[,] _dist;
        public float[,] Dist
        {
            get
            {
                if (!_distTableUpdated) UpdateDistanceTable();
                return _dist;
            }
        }

        public override void BeforeUpdateActions()
        {
            foreach (var member in Members)
            {
                member.BeforeUpdateActions();
            }
        }

        public override void AfterLateUpdateActions()
        {
            _distTableUpdated = false;

            foreach (var member in Members)
            {
                member.AfterLateUpdateActions();
            }
        }

        private void UpdateDistanceTable()
        {
            var len = Members.Count;
            _dist = new float[len, len];
            for (var i = 0; i < len; i++)
            {
                for (var j = 0; j < len; j++)
                {
                    _dist[i, j] = Vector3.Distance(Members[i].transform.position, Members[j].transform.position);
                }
            }
            _distTableUpdated = true;
        }

        public Vector3 GetGroupCenter()
        {
            if (Members.Count == 0) return Vector3.zero;

            var center = Vector3.zero;
            foreach (var member in Members)
            {
                center += member.transform.position;
            }

            return center / Members.Count;
        }

        /// <summary>
        /// Finds the center of the agents inside a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns></returns>
        public Vector3 GetHoodCenter(Vector3 center, float radius)
        {
            var hoodMemberCount = 0;
            var hoodCenter = Vector3.zero;

            foreach (var member in Members)
            {
                if (Vector3.SqrMagnitude(member.transform.position - center) <= radius * radius)
                {
                    hoodMemberCount++;
                    hoodCenter += member.transform.position;
                }
            }

            return hoodMemberCount > 0 ? hoodCenter / hoodMemberCount : hoodCenter;
        }

        /// <summary>
        /// Finds the agents inside a circle.
        /// </summary>
        /// <param name="center">The circle's center.</param>
        /// <param name="radius">The circle's radius.</param>
        /// <returns>A list of agent ids.</returns>
        public List<int> GetMemberIdsInCircle(Vector3 center, float radius)
        {
            var idsList = new List<int>();

            for (var i = 0; i < Members.Count; i++)
            {
                if (Vector3.SqrMagnitude(Members[i].transform.position - center) <= radius * radius)
                {
                    idsList.Add(i);
                }
            }

            return idsList;
        }

        /// <summary>
        /// Positions the group members to a grid.
        /// </summary>
        /// <param name="columns">The grid's columns.</param>
        /// <param name="bottomLeftPoint">The position of the bottom left agent in the grid.</param>
        /// <param name="rowDistance">The distance between two rows.</param>
        /// <param name="colDistance">The distance between two columns.</param>
        public void ToGridFormation(int columns, Vector3 bottomLeftPoint, float rowDistance, float colDistance)
        {
            var currentRow = 0;
            var currentCol = 0;
            foreach(var member in Members) 
            {
                member.transform.position = bottomLeftPoint + currentRow * rowDistance * Vector3.right + currentCol * colDistance * Vector3.forward;
                currentCol++;
                if (currentCol == columns) {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }

        /// <summary>
        /// Sets the state of the specified members to one. Reset all the others' to zero.
        /// </summary>
        /// <param name="activeIds">A list of the members' ids to set their state to 1.</param> 
        public void SetState(List<int> activeIds)
        {
            foreach(var member in Members)
            {
                member.State = 0;
            }
            foreach(var id in activeIds)
            {
                Members[id].State = 1;
            }
        }

        /// <summary>
        /// Sets up all the member's neighbours for a grid formation.
        /// </summary>
        /// <param name="columns">The grid's columns.</param>
        public void RegisterGridNeighbours(int gridColumns)
        {
            if (gridColumns <= 0) 
            {
                Debug.LogError("The number of columns must be positive.");
                return;
            }
            var maxId = Members.Count;
            var maxRow = maxId / gridColumns;

            for (var i = 0; i < maxId; i++)
            {
                Members[i].Neighbours = new List<int>(8);
                var row = i / gridColumns;
                var column = i % gridColumns;
                for (var k = -1; k < 2; k++) 
                {
                    var currentRow = row + k;
                    if (currentRow < 0 || currentRow > maxRow) continue;
                    for (var l = -1; l < 2; l++) 
                    {
                        var currentCol = column + l;
                        if (currentCol < 0 || currentCol >= gridColumns) continue;

                        var id = currentRow * gridColumns + currentCol;
                        if (id != i && id < maxId) Members[i].Neighbours.Add(id);
                    }
                }
            }
        }

        public void UpdateStates(string algorithm, string algorithmData)
        {
            if (algorithm == "gameoflife") 
            {
                var sMatch = Regex.Match(algorithmData, @"S([0-9])+", RegexOptions.Compiled);
                var bMatch = Regex.Match(algorithmData, @"B([0-9])+", RegexOptions.Compiled);
                var surviveList = new List<int>();
                var bornList = new List<int>();
                if (sMatch.Groups.Count == 2) 
                {
                    foreach (Capture capture in sMatch.Groups[1].Captures)
                    {
                        surviveList.Add(int.Parse(capture.ToString()));
                    }
                }
                
                if (bMatch.Groups.Count == 2) 
                {
                    foreach (Capture capture in bMatch.Groups[1].Captures)
                    {
                        bornList.Add(int.Parse(capture.ToString()));
                    }
                }
                var states = new List<int>(Members.Count);
                foreach (var member in Members)
                {
                    states.Add(member.State);
                }
                foreach (var member in Members)
                {
                    var activeNeigbours = 0;
                    foreach (var id in member.Neighbours)
                    {
                        if (states[id] != 0) 
                        {
                            activeNeigbours++;
                        }
                    }
                    if (member.State == 0 && bornList.Contains(activeNeigbours))
                    {
                        member.State = 1;
                    }
                    else if (member.State == 1 && !surviveList.Contains(activeNeigbours))
                    {
                        member.State = 0;
                    }
                }
            }
            else
            {
                Debug.LogError("Not supported algorithm. Only 'gameoflife' is currently supported.");
            }
        }

        /// <summary>
        /// Shows/Hides the index of each member. Useful for debugging.
        /// </summary>
        /// <param name="show">Show the members' ids.</param>
        public void ToggleIndices(bool show)
        {
            foreach(var member in Members)
            {
                member.ToggleTextMeshObject(show);
                if (show) member.SetText(member.GroupMemberId.ToString());
            }
        }

        /// <summary>
        /// Highlights the neighbours of a specific group member.
        /// </summary>
        /// <param name="memberId">The member's id.</param>
        public void HighlightNeigbours(int memberId)
        {
            foreach(var member in Members)
            {
                member.ToggleTextMeshObject(true);
                member.TextMeshComponent.color = new Color(1, 1, 1);
            }

            foreach (var id in Members[memberId].Neighbours)
            {
                Members[id].TextMeshComponent.color = new Color(0, 1, 0);
            }
        }

        public Vector3 DirOfAgent(int agentId)
        {
            Debug.Assert(Members.Count > agentId && agentId >= 0);

            return Members[agentId].DirMine();
        }

        public int GetNearestAgentWithState(Vector3 position, int state)
        {
            var minDist = float.MaxValue;
            var minId = -1;
            for (var i = 0; i < Members.Count; i++)
            {
                var dist = Vector3.SqrMagnitude(Members[i].transform.position - position);
                if (Members[i].State == state && dist < minDist) 
                {
                    minDist = dist;
                    minId = i;
                }
            }
            return minId;
        }

        public float DistOfAgents(int agentId1, int agentId2)
        {
            Debug.Assert(Members.Count > agentId1 && agentId1 >= 0);
            Debug.Assert(Members.Count > agentId2 && agentId2 >= 0);
            
            return Vector3.Distance(Members[agentId1].transform.position, Members[agentId2].transform.position);
        }
    }
}