﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;

namespace WarriorRoad {
	public class CMapManager : CMonoSingleton<CMapManager> {

		[Header ("Map size")]
		[SerializeField]	private int m_MapSize = 5;

		[Header ("Map block")]
		[SerializeField]	private CBlockController[] m_NormalBlock;
		[SerializeField]	private CBlockController m_LeftTopBlock;
		[SerializeField]	private CBlockController m_RightTopBlock;
		[SerializeField]	private CBlockController m_LeftBottomBlock;
		[SerializeField]	private CBlockController m_RightBottomBlock;

		[Header ("Monster")]
		[SerializeField]	private GameObject[] m_Monsters;

		[Header ("Map Control")]
		[SerializeField]	private CBlockController[] m_Blocks;

		public System.Action<float> OnMapGenerateProcess;
		public System.Action OnMapGenerateComplete;

		public CBlockController CalculateCurrentBlock (int index) {
			if (this.m_Blocks == null)
				return null;
			var fitIndex = index % this.m_Blocks.Length;
			return this.m_Blocks[fitIndex];
		}

		public virtual void GenerateRoadMap() {
			this.m_Blocks = new CBlockController[(this.m_MapSize - 1) * 4];
			var blockX = 0;
			var blockY = 0;
			var tmpIndex = 0;
			for (int i = 0; i < this.m_Blocks.Length; i++) {
//				// LEFT
				if (i < this.m_MapSize - 1) {
					blockX = 0;
					blockY = i;
				} 
				// TOP
				if (i >= this.m_MapSize - 1 && i < (this.m_MapSize - 1) * 2) {
					blockX = tmpIndex;
					blockY = this.m_MapSize - 1;
					tmpIndex++;
				}
				// RIGHT
				if (i >= (this.m_MapSize - 1) * 2 && i < (this.m_MapSize - 1) * 3) {
					blockX = this.m_MapSize - 1;
					blockY = this.m_MapSize - 1 - tmpIndex;
					tmpIndex++;
				}
				// BOTTOM
				if (i >= (this.m_MapSize - 1) * 3 && i < (this.m_MapSize - 1) * 4) {
					blockX = this.m_MapSize - 1 - tmpIndex;
					blockY = 0;
					tmpIndex++;
				}
				tmpIndex = tmpIndex % (this.m_MapSize - 1);
				var block = GetBlockBaseIndex (blockX, blockY);
				StartCoroutine (this.HandleSpawnBlock(i, block, blockX, blockY));
			}
		}

		public virtual void ClearMap() {
			var childCount = this.transform.childCount;
			for (int i = 0; i < childCount; i++) {
				var childGO = this.transform.GetChild (i).gameObject;
				GameObject.Destroy (childGO);
			}
			this.m_Blocks = null;
		}

		private IEnumerator HandleSpawnBlock(int index, CBlockController block, int x, int y) {
			var spawnedBlock = Instantiate (block);
			spawnedBlock.name = string.Format ("Block {0}-{1}", x, y);
			yield return spawnedBlock != null;
			spawnedBlock.transform.SetParent (this.transform);
			spawnedBlock.transform.position = this.GetBlockPosition (x, y);
			spawnedBlock.transform.rotation = this.GetBlockRotation (x, y);
			spawnedBlock.SetActive (true);
			yield return this.HandleSpawnMonster (spawnedBlock);
			this.m_Blocks [index] = spawnedBlock;
			if (this.OnMapGenerateProcess != null) {
				this.OnMapGenerateProcess ((float) index / this.m_Blocks.Length);
			}
			if (index == this.m_Blocks.Length - 1) {
				if (this.OnMapGenerateComplete != null) {
					this.OnMapGenerateComplete ();
				}
			}
		}

		private IEnumerator HandleSpawnMonster(CBlockController parent) {
			if (parent.enemyPoint != null) {
				var monsterPrefabIndex = Random.Range (0, this.m_Monsters.Length); 
				var monsterGO = Instantiate (this.m_Monsters [monsterPrefabIndex]);
				yield return monsterGO != null;
				monsterGO.transform.SetParent (parent.enemyPoint.transform);
				monsterGO.transform.localPosition = Vector3.zero;
				monsterGO.transform.localRotation = Quaternion.identity;
			}
			yield return null;
		}

		private Quaternion GetBlockRotation(int x, int y) {
			var eulerV3 = Vector3.zero;
			if (x == 0 && y > 0 && y < this.m_MapSize - 1) {
				eulerV3.y = -90f;
			} else if (x > 0 && x < this.m_MapSize - 1 && y == 0) {
				eulerV3.y = 180f;
			} else if (x == this.m_MapSize - 1 && y > 0 && y < this.m_MapSize - 1) {
				eulerV3.y = 90f;
			} else if (x > 0 && x < this.m_MapSize - 1 && y == this.m_MapSize - 1) {
				eulerV3.y = 0f;
			}
			return Quaternion.Euler (eulerV3);
		}

		private Vector3 GetBlockPosition(int x, int y) {
			var blockX = x - ((this.m_MapSize -1) / 2f);
			var blockY = y - ((this.m_MapSize - 1) / 2f);
			return new Vector3 (blockX, 0, blockY);
		}

		private CBlockController GetBlockBaseIndex(int x, int y) {
			if (this.m_Blocks == null)
				return null;
			if (x == 0 && y == 0) {
				return this.m_LeftBottomBlock;
			} 
			if (x == this.m_MapSize - 1 && y == 0) {
				return this.m_RightBottomBlock;
			}
			if (x == 0 && y == this.m_MapSize - 1) {
				return this.m_LeftTopBlock;
			}
			if (x == this.m_MapSize - 1 && y == this.m_MapSize - 1) {
				return this.m_RightTopBlock;
			}
			var randomIndex = Random.Range (0, this.m_NormalBlock.Length);
			return this.m_NormalBlock[randomIndex];
		}
		
	}
}