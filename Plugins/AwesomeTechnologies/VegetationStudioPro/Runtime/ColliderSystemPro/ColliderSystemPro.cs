using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.ColliderSystem
{    
    [AwesomeTechnologiesScriptOrder(105)]
    [ExecuteInEditMode]
    public class ColliderSystemPro : MonoBehaviour
    {
        public VegetationSystemPro VegetationSystemPro;

        [NonSerialized] public VisibleVegetationCellSelector VisibleVegetationCellSelector;  
        [NonSerialized] public readonly List<VegetationPackageColliderInfo> PackageColliderInfoList = new List<VegetationPackageColliderInfo>(); 
        public NativeList<JobHandle> JobHandleList;
        
        public int CurrentTabIndex;
        public bool ShowDebugCells;
        private Transform _colliderParent;
        public bool ShowColliders;

        private void Reset()
        {
            FindVegetationSystemPro();
        }

        private void FindVegetationSystemPro()
        {
            if (!VegetationSystemPro)
            {
                VegetationSystemPro = GetComponent<VegetationSystemPro>();
            }
        }

        private void OnEnable()
        {
            FindVegetationSystemPro();
            SetupDelegates();
            SetupColliderSystem();
        }

        private void SetupDelegates()
        {
            if (!VegetationSystemPro) return;
            
            VegetationSystemPro.OnRefreshVegetationSystemDelegate += OnRefreshVegetationSystem;
            VegetationSystemPro.OnRefreshColliderSystemDelegate += OnRefreshVegetationSystem;

            VegetationSystemPro.OnClearCacheDelegate += OnClearCache;
            VegetationSystemPro.OnClearCacheVegetationCellDelegate += OnClearCacheVegetationCell;
            VegetationSystemPro.OnClearCacheVegetationItemDelegate += OnClearCacheVegetationItem;
            VegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate +=
                OnClearCacheVegetationCellVegetationItem;
            VegetationSystemPro.OnRenderCompleteDelegate += OnRenderComplete;
        }

        private void RemoveDelegates()
        {
            if (!VegetationSystemPro) return;
            
            VegetationSystemPro.OnRefreshVegetationSystemDelegate -= OnRefreshVegetationSystem;
            VegetationSystemPro.OnRefreshColliderSystemDelegate -= OnRefreshVegetationSystem;

            VegetationSystemPro.OnClearCacheDelegate -= OnClearCache;
            VegetationSystemPro.OnClearCacheVegetationCellDelegate -= OnClearCacheVegetationCell;
            VegetationSystemPro.OnClearCacheVegetationItemDelegate -= OnClearCacheVegetationItem;
            VegetationSystemPro.OnClearCacheVegetationCellVegetatonItemDelegate -=
                OnClearCacheVegetationCellVegetationItem;
            VegetationSystemPro.OnRenderCompleteDelegate -= OnRenderComplete;
        }

        private void OnDisable()
        {
            DisposeColliderSystem();
            RemoveDelegates();
        }
        
        public void SetColliderVisibility(bool value)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.SetColliderVisibility(value);
                }
            }  
        }

        private void OnClearCache(VegetationSystemPro vegetationSystemPro)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.VegetationItemSelector.RefreshAllVegetationCells();
                }
            }   
        }

        private void OnClearCacheVegetationCell(VegetationSystemPro vegetationSystemPro, VegetationCell vegetationCell)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                }
            }    
        }

        private void OnClearCacheVegetationItem(VegetationSystemPro vegetationSystemPro, int vegetationPackageIndex,
            int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                        colliderManager?.VegetationItemSelector.RefreshAllVegetationCells(); 
                    }
                }
            }    
        }

        private void OnClearCacheVegetationCellVegetationItem(VegetationSystemPro vegetationSystemPro,
            VegetationCell vegetationCell, int vegetationPackageIndex, int vegetationItemIndex)
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    if (i == vegetationPackageIndex && j == vegetationItemIndex)
                    {
                        ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                        colliderManager?.VegetationItemSelector.RefreshVegetationCell(vegetationCell);
                    }
                }
            }    
        }

        private void OnRefreshVegetationSystem(VegetationSystemPro vegetationSystemPro)
        {
            SetupColliderSystem();
        }

        public void UpdateCullingDistance()
        {
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.UpdateColliderDistance();
                }
            }    
        }

        public void SetupColliderSystem()
        {
            if (!VegetationSystemPro) return;
                        
            DisposeColliderSystem();
            
            JobHandleList = new NativeList<JobHandle>(64,Allocator.Persistent);
            
            CreateColliderParent();

            VisibleVegetationCellSelector = new VisibleVegetationCellSelector();

            for (int i = 0; i <= VegetationSystemPro.VegetationPackageProList.Count - 1; i++)
            {
                VegetationPackagePro vegetationPackagePro = VegetationSystemPro.VegetationPackageProList[i];
                VegetationPackageColliderInfo vegetationPackageColliderInfo = new VegetationPackageColliderInfo();

                for (int j = 0; j <= vegetationPackagePro.VegetationInfoList.Count - 1; j++)
                {
                    VegetationItemInfoPro vegetationItemInfoPro = vegetationPackagePro.VegetationInfoList[j];
                    if (vegetationItemInfoPro.ColliderType != ColliderType.Disabled)
                    {                       
                        ColliderManager tmpColliderManager = new ColliderManager(VisibleVegetationCellSelector,VegetationSystemPro,vegetationItemInfoPro,_colliderParent,ShowColliders); 
                        vegetationPackageColliderInfo.ColliderManagerList.Add(tmpColliderManager);
                    }
                    else
                    {
                        vegetationPackageColliderInfo.ColliderManagerList.Add(null);
                    }
                }
                
                PackageColliderInfoList.Add(vegetationPackageColliderInfo);
            }        
            VisibleVegetationCellSelector.Init(VegetationSystemPro);            
        }

        void CreateColliderParent()
        {
            GameObject colliderParentObject = new GameObject("Run-time colliders") {hideFlags = HideFlags.DontSave};
            colliderParentObject.transform.SetParent(transform);
            _colliderParent = colliderParentObject.transform;
        }

        void DestroyColliderParent()
        {
            if (!_colliderParent) return;

            if (Application.isPlaying)
            {
                Destroy(_colliderParent.gameObject);
            }
            else
            {
                DestroyImmediate(_colliderParent.gameObject);
            }
        }

        private void OnRenderComplete(VegetationSystemPro vegetationSystemPro)
        {
            if (PackageColliderInfoList.Count == 0) return;

            Profiler.BeginSample("Collider system processing");
            JobHandleList.Clear();
            JobHandle cullingJobHandle = default(JobHandle);
                       
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager == null) continue;

                    JobHandle itemCullingHandle = cullingJobHandle;
                    
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessInvisibleCells(itemCullingHandle);
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessVisibleCells(itemCullingHandle);
                    itemCullingHandle =  colliderManager.VegetationItemSelector.ProcessCulling(itemCullingHandle);
                    JobHandleList.Add(itemCullingHandle);
                }
            }
            
            JobHandle mergedHandle = JobHandle.CombineDependencies(JobHandleList);            
            mergedHandle.Complete();
            
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                  ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                  colliderManager?.VegetationItemSelector.ProcessEvents();
                }
            }           
            Profiler.EndSample();
        }

        public void DisposeColliderSystem()
        {
            if (JobHandleList.IsCreated)
            {
                JobHandleList.Dispose();
            }
            
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    colliderManager?.Dispose();
                }

                vegetationPackageColliderInfo.ColliderManagerList.Clear();
            }
            PackageColliderInfoList.Clear();
            VisibleVegetationCellSelector?.Dispose();
            VisibleVegetationCellSelector = null;
            DestroyColliderParent();
        }

        public int GetLoadedInstanceCount()
        {
            int instanceCount = 0;
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        instanceCount += colliderManager.VegetationItemSelector.InstanceList.Length;
                    }
                }
            }
            return instanceCount;
        }

        public int GetVisibleColliders()
        {
            int instanceCount = 0;
            for (int i = 0; i <= PackageColliderInfoList.Count - 1; i++)
            {
                VegetationPackageColliderInfo vegetationPackageColliderInfo = PackageColliderInfoList[i];
                for (int j = 0; j <= vegetationPackageColliderInfo.ColliderManagerList.Count - 1; j++)
                {
                    ColliderManager colliderManager = vegetationPackageColliderInfo.ColliderManagerList[j];
                    if (colliderManager != null)
                    {
                        instanceCount += colliderManager.RuntimePrefabStorage.RuntimePrefabInfoList.Count;
                    }
                }
            }
            return instanceCount;
        }        

        private void OnDrawGizmosSelected()
        {
            if (ShowDebugCells)
            {
                VisibleVegetationCellSelector?.DrawDebugGizmos();
            }
        }
    }
}