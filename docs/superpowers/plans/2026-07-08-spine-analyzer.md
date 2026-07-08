# Spine Analyzer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a read-only Unity Editor analyzer for selected spine-unity assets and objects.

**Architecture:** Keep analysis logic separate from the Editor Window. `SpineAssetAnalyzer` returns a simple `SpineAnalysisReport`; `SpineAnalyzerWindow` only resolves selection and renders the report. Tests exercise the analyzer service directly.

**Tech Stack:** Unity `2022.3.62f2`, spine-unity `4.3.95`, spine-csharp `4.3.38`, NUnit EditMode tests.

## Global Constraints

- Keep the analyzer read-only; do not mutate imported assets, prefab contents, scene objects, or project settings.
- Keep runtime optimization and Runtime LOD out of this milestone.
- Do not claim measured performance wins from static analysis.
- Keep code under the existing `OptimizedSpine.Editor` and `OptimizedSpine.EditorTests` assemblies.
- Use the existing official sample `spineboy-pro_SkeletonData.asset` for integration-style EditMode tests.

---

## File Structure

- Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisSeverity.cs`
  - Defines `Info`, `Warning`, `Critical`.
- Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisFinding.cs`
  - Immutable finding data: severity, title, details, suggestion.
- Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisReport.cs`
  - Report data: target identity, metrics, renderer settings, ordered findings.
- Create `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs`
  - Pure analyzer service. Accepts `UnityEngine.Object` and returns `SpineAnalysisReport`.
- Create `Assets/OptimizedSpine/Editor/SpineAnalyzerWindow.cs`
  - Editor UI and menu item `OptimizedSpine/Spine Analyzer`.
- Modify `Assets/OptimizedSpine/Tests/EditMode/OptimizedSpine.EditorTests.asmdef`
  - Add reference to `OptimizedSpine.Editor`.
- Create `Assets/OptimizedSpine/Tests/EditMode/SpineAssetAnalyzerTests.cs`
  - EditMode tests for analyzer behavior.
- Modify `README.md`
  - Add analyzer usage.
- Modify `docs/project-memory.md`
  - Record analyzer milestone and verification.

---

### Task 1: Analyzer Data Model And Red Tests

**Files:**
- Create: `Assets/OptimizedSpine/Tests/EditMode/SpineAssetAnalyzerTests.cs`
- Modify: `Assets/OptimizedSpine/Tests/EditMode/OptimizedSpine.EditorTests.asmdef`
- Create: `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisSeverity.cs`
- Create: `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisFinding.cs`
- Create: `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisReport.cs`
- Create: `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs`

**Interfaces:**
- Produces: `SpineAssetAnalyzer.Analyze(Object target): SpineAnalysisReport`
- Produces: `SpineAnalysisReport.HasCriticalFindings: bool`
- Produces: `SpineAnalysisReport.Findings: IReadOnlyList<SpineAnalysisFinding>`

- [ ] **Step 1: Add failing tests for null and unsupported targets**

Add this first version of `Assets/OptimizedSpine/Tests/EditMode/SpineAssetAnalyzerTests.cs`:

```csharp
using NUnit.Framework;
using OptimizedSpine.EditorTools.Analysis;
using UnityEngine;

namespace OptimizedSpine.Tests
{
    public sealed class SpineAssetAnalyzerTests
    {
        [Test]
        public void Analyze_NullTarget_ReturnsCriticalReport()
        {
            SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(null);

            Assert.That(report.Analyzed, Is.False);
            Assert.That(report.HasCriticalFindings, Is.True);
            Assert.That(report.Findings[0].Severity, Is.EqualTo(SpineAnalysisSeverity.Critical));
        }

        [Test]
        public void Analyze_UnsupportedObject_ReturnsCriticalReport()
        {
            Texture2D texture = new Texture2D(1, 1);

            try
            {
                SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(texture);

                Assert.That(report.Analyzed, Is.False);
                Assert.That(report.TargetKind, Is.EqualTo("Unsupported"));
                Assert.That(report.HasCriticalFindings, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
```

- [ ] **Step 2: Reference the editor assembly from tests**

Modify `Assets/OptimizedSpine/Tests/EditMode/OptimizedSpine.EditorTests.asmdef` references to include `OptimizedSpine.Editor`:

```json
"references": [
  "OptimizedSpine.Runtime",
  "OptimizedSpine.Editor",
  "spine-unity",
  "spine-csharp"
]
```

- [ ] **Step 3: Run tests and verify RED**

Run EditMode tests through Unity Test Runner or UnitySkills if available:

```powershell
# Preferred when UnitySkills allows test execution:
# run EditMode tests for OptimizedSpine.EditorTests
```

Expected: compile/test failure because `OptimizedSpine.EditorTools.Analysis` and `SpineAssetAnalyzer` do not exist yet.

- [ ] **Step 4: Add minimal data model and analyzer shell**

Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisSeverity.cs`:

```csharp
namespace OptimizedSpine.EditorTools.Analysis
{
    public enum SpineAnalysisSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }
}
```

Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisFinding.cs`:

```csharp
namespace OptimizedSpine.EditorTools.Analysis
{
    public sealed class SpineAnalysisFinding
    {
        public SpineAnalysisFinding(SpineAnalysisSeverity severity, string title, string details, string suggestion)
        {
            Severity = severity;
            Title = title;
            Details = details;
            Suggestion = suggestion;
        }

        public SpineAnalysisSeverity Severity { get; }
        public string Title { get; }
        public string Details { get; }
        public string Suggestion { get; }
    }
}
```

Create `Assets/OptimizedSpine/Editor/Analysis/SpineAnalysisReport.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace OptimizedSpine.EditorTools.Analysis
{
    public sealed class SpineAnalysisReport
    {
        private readonly List<SpineAnalysisFinding> findings = new List<SpineAnalysisFinding>();

        public string TargetName { get; set; } = "None";
        public string TargetKind { get; set; } = "Unsupported";
        public bool Analyzed { get; set; }
        public int AtlasAssetCount { get; set; }
        public int MaterialCount { get; set; }
        public int SlotCount { get; set; }
        public int SkinCount { get; set; }
        public int AnimationCount { get; set; }
        public int AttachmentCount { get; set; }
        public bool HasRendererSettings { get; set; }
        public bool SingleSubmesh { get; set; }
        public bool ImmutableTriangles { get; set; }
        public string UpdateWhenInvisible { get; set; } = "Unavailable";

        public IReadOnlyList<SpineAnalysisFinding> Findings => findings;
        public bool HasCriticalFindings => findings.Any(finding => finding.Severity == SpineAnalysisSeverity.Critical);

        public void AddFinding(SpineAnalysisSeverity severity, string title, string details, string suggestion)
        {
            findings.Add(new SpineAnalysisFinding(severity, title, details, suggestion));
            findings.Sort((left, right) => right.Severity.CompareTo(left.Severity));
        }
    }
}
```

Create `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs` with only null and unsupported handling:

```csharp
using UnityEngine;

namespace OptimizedSpine.EditorTools.Analysis
{
    public static class SpineAssetAnalyzer
    {
        public static SpineAnalysisReport Analyze(Object target)
        {
            if (target == null)
                return Critical("None", "Unsupported", "No target selected", "Select a SkeletonDataAsset or a GameObject with SkeletonAnimation.");

            return Critical(target.name, "Unsupported", "Unsupported target", "Select a SkeletonDataAsset, SkeletonAnimation, SkeletonRenderer, or compatible GameObject.");
        }

        private static SpineAnalysisReport Critical(string targetName, string targetKind, string title, string suggestion)
        {
            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = targetName,
                TargetKind = targetKind,
                Analyzed = false
            };

            report.AddFinding(SpineAnalysisSeverity.Critical, title, "The analyzer could not resolve Spine skeleton data from this selection.", suggestion);
            return report;
        }
    }
}
```

- [ ] **Step 5: Run tests and verify GREEN**

Expected: the two new tests pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/OptimizedSpine/Editor/Analysis Assets/OptimizedSpine/Tests/EditMode
git commit -m "test: add spine analyzer report basics"
```

---

### Task 2: Analyze Skeleton Data And Components

**Files:**
- Modify: `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs`
- Modify: `Assets/OptimizedSpine/Tests/EditMode/SpineAssetAnalyzerTests.cs`

**Interfaces:**
- Consumes: `SpineAssetAnalyzer.Analyze(Object target): SpineAnalysisReport`
- Produces: metrics on `SpineAnalysisReport`

- [ ] **Step 1: Add failing tests for `SkeletonDataAsset` and `SkeletonAnimation` resolution**

Append tests:

```csharp
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
```

Add test constant:

```csharp
private const string DefaultSkeletonPath =
    "Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset";
```

Add tests:

```csharp
[Test]
public void Analyze_SkeletonDataAsset_ReturnsMetrics()
{
    SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
    Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

    SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(asset);

    Assert.That(report.Analyzed, Is.True);
    Assert.That(report.TargetKind, Is.EqualTo("SkeletonDataAsset"));
    Assert.That(report.SlotCount, Is.GreaterThan(0));
    Assert.That(report.AnimationCount, Is.GreaterThan(0));
    Assert.That(report.SkinCount, Is.GreaterThan(0));
    Assert.That(report.AttachmentCount, Is.GreaterThan(0));
}

[Test]
public void Analyze_GameObjectWithSkeletonAnimation_UsesComponentSkeletonData()
{
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
    Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

    GameObject owner = new GameObject("AnalyzerSkeleton");
    owner.SetActive(false);

    try
    {
        SkeletonRenderer renderer = owner.AddComponent<SkeletonRenderer>();
        SkeletonAnimation animation = owner.AddComponent<SkeletonAnimation>();
        renderer.SkeletonDataAsset = asset;
        renderer.Animation = animation;

        SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(owner);

        Assert.That(report.Analyzed, Is.True);
        Assert.That(report.TargetKind, Is.EqualTo("GameObject"));
        Assert.That(report.TargetName, Is.EqualTo("AnalyzerSkeleton"));
        Assert.That(report.HasRendererSettings, Is.True);
    }
    finally
    {
        Object.DestroyImmediate(owner);
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Expected: tests fail because supported target resolution and metrics are not implemented.

- [ ] **Step 3: Implement target resolution and skeleton metrics**

Update `SpineAssetAnalyzer` to:

```csharp
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OptimizedSpine.EditorTools.Analysis
{
    public static class SpineAssetAnalyzer
    {
        private const int HighSlotCount = 80;
        private const int HighAttachmentCount = 160;

        public static SpineAnalysisReport Analyze(Object target)
        {
            if (target == null)
                return Critical("None", "Unsupported", "No target selected", "Select a SkeletonDataAsset or a GameObject with SkeletonAnimation.");

            AnalysisTarget resolved = ResolveTarget(target);
            if (resolved.SkeletonDataAsset == null)
                return Critical(target.name, resolved.TargetKind, "Missing SkeletonDataAsset", "Assign a SkeletonDataAsset before analyzing this target.");

            SkeletonData skeletonData = resolved.SkeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
                return Critical(target.name, resolved.TargetKind, "SkeletonData is not loaded", "Reimport the Spine asset or check import errors in the Console.");

            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = target.name,
                TargetKind = resolved.TargetKind,
                Analyzed = true,
                AtlasAssetCount = resolved.SkeletonDataAsset.atlasAssets != null ? resolved.SkeletonDataAsset.atlasAssets.Length : 0,
                MaterialCount = CountMaterials(resolved.SkeletonDataAsset),
                SlotCount = skeletonData.Slots.Count,
                SkinCount = skeletonData.Skins.Count,
                AnimationCount = skeletonData.Animations.Count,
                AttachmentCount = CountAttachments(skeletonData)
            };

            ApplyRendererSettings(report, resolved.Renderer);
            AddFindings(report);
            return report;
        }

        private static AnalysisTarget ResolveTarget(Object target)
        {
            if (target is SkeletonDataAsset skeletonDataAsset)
                return new AnalysisTarget("SkeletonDataAsset", skeletonDataAsset, null);

            if (target is SkeletonAnimation skeletonAnimation)
                return new AnalysisTarget("SkeletonAnimation", skeletonAnimation.SkeletonDataAsset, skeletonAnimation.Renderer as SkeletonRenderer);

            if (target is SkeletonRenderer skeletonRenderer)
                return new AnalysisTarget("SkeletonRenderer", skeletonRenderer.SkeletonDataAsset, skeletonRenderer);

            if (target is GameObject gameObject)
            {
                SkeletonAnimation childAnimation = gameObject.GetComponentInChildren<SkeletonAnimation>(true);
                if (childAnimation != null)
                    return new AnalysisTarget("GameObject", childAnimation.SkeletonDataAsset, childAnimation.Renderer as SkeletonRenderer);

                SkeletonRenderer childRenderer = gameObject.GetComponentInChildren<SkeletonRenderer>(true);
                if (childRenderer != null)
                    return new AnalysisTarget("GameObject", childRenderer.SkeletonDataAsset, childRenderer);

                return new AnalysisTarget("GameObject", null, null);
            }

            return new AnalysisTarget("Unsupported", null, null);
        }

        private static int CountMaterials(SkeletonDataAsset skeletonDataAsset)
        {
            HashSet<Material> materials = new HashSet<Material>();
            if (skeletonDataAsset.atlasAssets == null)
                return 0;

            foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets)
            {
                if (atlasAsset == null)
                    continue;

                Material[] atlasMaterials = atlasAsset.Materials;
                if (atlasMaterials == null)
                    continue;

                foreach (Material material in atlasMaterials)
                {
                    if (material != null)
                        materials.Add(material);
                }
            }

            return materials.Count;
        }

        private static int CountAttachments(SkeletonData skeletonData)
        {
            int count = 0;
            List<Skin.SkinEntry> entries = new List<Skin.SkinEntry>();

            foreach (Skin skin in skeletonData.Skins)
            {
                for (int slotIndex = 0; slotIndex < skeletonData.Slots.Count; slotIndex++)
                {
                    entries.Clear();
                    skin.GetAttachments(slotIndex, entries);
                    count += entries.Count;
                }
            }

            return count;
        }

        private static void ApplyRendererSettings(SpineAnalysisReport report, SkeletonRenderer renderer)
        {
            if (renderer == null)
                return;

            report.HasRendererSettings = true;
            report.SingleSubmesh = renderer.singleSubmesh;
            report.ImmutableTriangles = renderer.MeshSettings.immutableTriangles;
            report.UpdateWhenInvisible = renderer.UpdateWhenInvisible.ToString();
        }

        private static void AddFindings(SpineAnalysisReport report)
        {
            if (report.MaterialCount > 1 || report.AtlasAssetCount > 1)
            {
                report.AddFinding(SpineAnalysisSeverity.Warning, "Multiple materials or atlas assets",
                    "This asset may require extra renderer submeshes or draw calls depending on draw order and blend mode usage.",
                    "Measure it in the benchmark scene before treating it as batch-friendly.");
            }
            else
            {
                report.AddFinding(SpineAnalysisSeverity.Info, "Single material path",
                    "The asset currently resolves to one material in static analysis.",
                    "This is usually easier to batch, but still verify with the benchmark scene.");
            }

            if (report.SlotCount >= HighSlotCount || report.AttachmentCount >= HighAttachmentCount)
            {
                report.AddFinding(SpineAnalysisSeverity.Info, "Complex skeleton",
                    $"Slots: {report.SlotCount}, attachments: {report.AttachmentCount}.",
                    "Use this asset in a 10/30/100 instance benchmark before making runtime optimization decisions.");
            }

            report.AddFinding(SpineAnalysisSeverity.Info, "Use Single Submesh",
                "Can reduce submeshes for compatible assets, but can be wrong for assets relying on separated materials or special render behavior.",
                "Treat it as an experiment toggle, not a default fix.");

            report.AddFinding(SpineAnalysisSeverity.Info, "Immutable Triangles",
                "Useful only when triangle topology stays stable for the active skin and animation set.",
                "Enable only after checking that attachments and clipping do not change topology unexpectedly.");

            report.AddFinding(SpineAnalysisSeverity.Info, "Update When Invisible",
                $"Current value: {report.UpdateWhenInvisible}.",
                "Disable offscreen updates unless gameplay needs invisible characters to keep animating.");
        }

        private static SpineAnalysisReport Critical(string targetName, string targetKind, string title, string suggestion)
        {
            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = targetName,
                TargetKind = targetKind,
                Analyzed = false
            };

            report.AddFinding(SpineAnalysisSeverity.Critical, title, "The analyzer could not resolve Spine skeleton data from this selection.", suggestion);
            return report;
        }

        private readonly struct AnalysisTarget
        {
            public AnalysisTarget(string targetKind, SkeletonDataAsset skeletonDataAsset, SkeletonRenderer renderer)
            {
                TargetKind = targetKind;
                SkeletonDataAsset = skeletonDataAsset;
                Renderer = renderer;
            }

            public string TargetKind { get; }
            public SkeletonDataAsset SkeletonDataAsset { get; }
            public SkeletonRenderer Renderer { get; }
        }
    }
}
```

- [ ] **Step 4: Run tests and verify GREEN**

Expected: analyzer tests pass.

- [ ] **Step 5: Commit**

```powershell
git add Assets/OptimizedSpine/Editor/Analysis Assets/OptimizedSpine/Tests/EditMode
git commit -m "feat: analyze spine skeleton assets"
```

---

### Task 3: Editor Window

**Files:**
- Create: `Assets/OptimizedSpine/Editor/SpineAnalyzerWindow.cs`

**Interfaces:**
- Consumes: `SpineAssetAnalyzer.Analyze(Object target): SpineAnalysisReport`

- [ ] **Step 1: Add the Editor Window**

Create `Assets/OptimizedSpine/Editor/SpineAnalyzerWindow.cs`:

```csharp
using OptimizedSpine.EditorTools.Analysis;
using UnityEditor;
using UnityEngine;

namespace OptimizedSpine.EditorTools
{
    public sealed class SpineAnalyzerWindow : EditorWindow
    {
        private Object target;
        private SpineAnalysisReport report;
        private Vector2 scroll;

        [MenuItem("OptimizedSpine/Spine Analyzer")]
        public static void Open()
        {
            GetWindow<SpineAnalyzerWindow>("Spine Analyzer");
        }

        private void OnEnable()
        {
            target = Selection.activeObject;
            AnalyzeSelection();
        }

        private void OnSelectionChange()
        {
            target = Selection.activeObject;
            AnalyzeSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spine Analyzer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField("Target", target, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
                AnalyzeSelection();

            if (GUILayout.Button("Analyze Selection"))
                AnalyzeSelection();

            EditorGUILayout.Space();

            if (report == null)
            {
                EditorGUILayout.HelpBox("Select a Spine asset or object to analyze.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary(report);
            DrawFindings(report);
            EditorGUILayout.EndScrollView();
        }

        private void AnalyzeSelection()
        {
            report = SpineAssetAnalyzer.Analyze(target);
        }

        private static void DrawSummary(SpineAnalysisReport report)
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name", report.TargetName);
            EditorGUILayout.LabelField("Type", report.TargetKind);
            EditorGUILayout.LabelField("Analyzed", report.Analyzed ? "Yes" : "No");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Static Metrics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Atlas Assets", report.AtlasAssetCount.ToString());
            EditorGUILayout.LabelField("Materials", report.MaterialCount.ToString());
            EditorGUILayout.LabelField("Slots", report.SlotCount.ToString());
            EditorGUILayout.LabelField("Skins", report.SkinCount.ToString());
            EditorGUILayout.LabelField("Animations", report.AnimationCount.ToString());
            EditorGUILayout.LabelField("Attachments", report.AttachmentCount.ToString());

            if (!report.HasRendererSettings)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Renderer Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use Single Submesh", report.SingleSubmesh ? "On" : "Off");
            EditorGUILayout.LabelField("Immutable Triangles", report.ImmutableTriangles ? "On" : "Off");
            EditorGUILayout.LabelField("Update When Invisible", report.UpdateWhenInvisible);
        }

        private static void DrawFindings(SpineAnalysisReport report)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Findings", EditorStyles.boldLabel);

            foreach (SpineAnalysisFinding finding in report.Findings)
            {
                MessageType messageType = ToMessageType(finding.Severity);
                EditorGUILayout.HelpBox($"{finding.Title}\n\n{finding.Details}\n\nSuggestion: {finding.Suggestion}", messageType);
            }
        }

        private static MessageType ToMessageType(SpineAnalysisSeverity severity)
        {
            switch (severity)
            {
                case SpineAnalysisSeverity.Critical:
                    return MessageType.Error;
                case SpineAnalysisSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
```

- [ ] **Step 2: Compile and smoke test**

Open Unity menu:

```text
OptimizedSpine/Spine Analyzer
```

Select:

```text
Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset
```

Expected: window shows non-zero metrics and findings.

- [ ] **Step 3: Commit**

```powershell
git add Assets/OptimizedSpine/Editor/SpineAnalyzerWindow.cs
git commit -m "feat: add spine analyzer editor window"
```

---

### Task 4: Documentation And Verification

**Files:**
- Modify: `README.md`
- Modify: `docs/project-memory.md`

- [ ] **Step 1: Update README**

Add the menu item and basic usage:

```markdown
- `OptimizedSpine/Spine Analyzer`: opens a read-only Editor Window for selected Spine assets or objects. It reports static metrics and candidate optimization hints; it does not auto-apply changes.
```

- [ ] **Step 2: Update project memory**

Record:

```markdown
## Analyzer Milestone

- Editor window: `OptimizedSpine/Spine Analyzer`
- Analyzer service: `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs`
- First version is read-only and reports static hints only.
- Runtime LOD remains a later milestone after benchmark measurements.
```

- [ ] **Step 3: Run verification**

Run the best available checks:

```powershell
git status --short
```

Run Unity compile diagnostics through UnitySkills if available. Run EditMode tests if UnitySkills is in Bypass mode or Unity Test Runner is otherwise available.

Expected:

- no C# compile errors
- analyzer EditMode tests pass when test execution is available
- Console error/warning count remains clean after compile

- [ ] **Step 4: Commit**

```powershell
git add README.md docs/project-memory.md
git commit -m "docs: document spine analyzer usage"
```
