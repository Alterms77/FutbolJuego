#!/usr/bin/env python3
"""Generate 9 Unity scene files for FutbolJuego."""
import os

SCENES_DIR = "/home/runner/work/FutbolJuego/FutbolJuego/Assets/Scenes"

# GUIDs
EVENTSYSTEM_GUID      = "f70555f144d8491a925f0cf6dedfc2a5"
STANDALONE_INPUT_GUID = "4f231c4fb786f3946a6b90b886c48677"
CANVAS_SCALER_GUID    = "0cd44c1031e13a943bb63640046fad76"
GRAPHIC_RAYCASTER_GUID= "dc42784cf147fd14cb166573786ce8a3"
IMAGE_GUID            = "fe87c0e1cc204ed48ad3b37840f39efc"
BUTTON_GUID           = "4e29b1a8efbd4b44bb3f3716e73f07ff"
TMP_TEXT_GUID         = "f4688fdb7655be2489b516fcd1b15b5c"
TMP_DROPDOWN_GUID     = "630efbe3b91f1fb4588db73c3f39df3d"
TMP_INPUTFIELD_GUID   = "d0b7e30e1ac3c454b9c4b8ed37e56b6c"
SLIDER_GUID           = "985f144b2b27ee14aad553e2e20b3e4f"

DashboardUI_GUID      = "1d0000000000000000000000000001b0"
SquadUI_GUID          = "1d0000000000000000000000000002e0"
TransferMarketUI_GUID = "1d0000000000000000000000000003c0"
ShopUI_GUID           = "1d0000000000000000000000000002d0"
TacticsUI_GUID        = "1d0000000000000000000000000002f0"
FinancesUI_GUID       = "1d0000000000000000000000000001c0"
CompetitionsUI_GUID   = "1d0000000000000000000000000001a0"
TeamSelectionUI_GUID  = "1d0000000000000000000000000003a0"
MatchDayUI_GUID       = "1d0000000000000000000000000002a0"

# ─────────────────────────────────────────────────────────────────────────────
# ID counter
# ─────────────────────────────────────────────────────────────────────────────
_ctr = [0]
def reset(): _ctr[0] = 1000000
def nid():
    v = _ctr[0]; _ctr[0] += 1; return v

# ─────────────────────────────────────────────────────────────────────────────
# Low-level YAML block builders
# ─────────────────────────────────────────────────────────────────────────────
def scene_header():
    return """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 10304, guid: 0000000000000000f000000000000000, type: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    accuratePlacementThreshold: 0
    maxJobWorkers: 0
    preserveTilesOnDelete: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}"""

def camera_block():
    return """--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 100001}
  - component: {fileID: 100002}
  - component: {fileID: 100003}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: -10}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!20 &100002
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {r: 0.07058824, g: 0.12156863, b: 0.2, a: 1}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_SensorSize: {x: 36, y: 24}
  m_LensShift: {x: 0, y: 0}
  m_FocalLength: 50
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 0
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {fileID: 0}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!81 &100003
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1"""

def event_system_block():
    return f"""--- !u!1 &200000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 200001}}
  - component: {{fileID: 200002}}
  - component: {{fileID: 200003}}
  m_Layer: 0
  m_Name: EventSystem
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &200001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &200002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {EVENTSYSTEM_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_FirstSelected: {{fileID: 0}}
  m_sendNavigationEvents: 1
  m_DragThreshold: 10
--- !u!114 &200003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {STANDALONE_INPUT_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_HorizontalAxis: Horizontal
  m_VerticalAxis: Vertical
  m_SubmitButton: Submit
  m_CancelButton: Cancel
  m_InputActionsPerSecond: 10
  m_RepeatDelay: 0.5
  m_ForceModuleActive: 0"""

# ─────────────────────────────────────────────────────────────────────────────
# Component block helpers  (return yaml string)
# ─────────────────────────────────────────────────────────────────────────────

def _go_block(fid, name, components, layer=5, active=True):
    comp_str = "\n".join(f"  - component: {{fileID: {c}}}" for c in components)
    return f"""--- !u!1 &{fid}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{comp_str}
  m_Layer: {layer}
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: {1 if active else 0}"""

def _rt_block(fid, go_fid, children, parent_fid, root_order,
              amin_x=0, amin_y=0, amax_x=1, amax_y=1,
              pos_x=0, pos_y=0, sz_x=0, sz_y=0, piv_x=0.5, piv_y=0.5):
    ch = "\n".join(f"  - {{fileID: {c}}}" for c in children) if children else "  []"
    if not children:
        ch_str = f"  m_Children: []"
    else:
        child_lines = "\n".join(f"  - {{fileID: {c}}}" for c in children)
        ch_str = f"  m_Children:\n{child_lines}"
    return f"""--- !u!224 &{fid}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
{ch_str}
  m_Father: {{fileID: {parent_fid}}}
  m_RootOrder: {root_order}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: {amin_x}, y: {amin_y}}}
  m_AnchorMax: {{x: {amax_x}, y: {amax_y}}}
  m_AnchoredPosition: {{x: {pos_x}, y: {pos_y}}}
  m_SizeDelta: {{x: {sz_x}, y: {sz_y}}}
  m_Pivot: {{x: {piv_x}, y: {piv_y}}}"""

def _canvas_renderer(fid, go_fid):
    return f"""--- !u!222 &{fid}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_CullTransparentMesh: 1"""

def _image(fid, go_fid, r=0.1, g=0.1, b=0.1, a=1):
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {IMAGE_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: {r}, g: {g}, b: {b}, a: {a}}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 0}}
  m_Type: 0
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1"""

def _tmp_text(fid, go_fid, text="", font_size=24, bold=False, center=True):
    h_align = 2 if center else 1
    weight = 700 if bold else 400
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {TMP_TEXT_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: "{text}"
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 0}}
  m_sharedMaterial: {{fileID: 0}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294967295
  m_fontColor: {{r: 1, g: 1, b: 1, a: 1}}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontColorGradient:
    topLeft: {{r: 1, g: 1, b: 1, a: 1}}
    topRight: {{r: 1, g: 1, b: 1, a: 1}}
    bottomLeft: {{r: 1, g: 1, b: 1, a: 1}}
    bottomRight: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontColorGradientPreset: {{fileID: 0}}
  m_spriteAsset: {{fileID: 0}}
  m_tintAllSprites: 0
  m_StyleSheet: {{fileID: 0}}
  m_TextStyleHashCode: -1183493901
  m_overrideHtmlColors: 0
  m_faceColor:
    serializedVersion: 2
    rgba: 4294967295
  m_fontSize: {font_size}
  m_fontSizeBase: {font_size}
  m_fontWeight: {weight}
  m_enableAutoSizing: 1
  m_fontSizeMin: 12
  m_fontSizeMax: {font_size}
  m_fontStyle: {1 if bold else 0}
  m_HorizontalAlignment: {h_align}
  m_VerticalAlignment: 256
  m_textAlignment: 65535
  m_characterSpacing: 0
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_enableWordWrapping: 1
  m_wordWrappingRatios: 0.4
  m_overflowMode: 0
  m_linkedTextComponent: {{fileID: 0}}
  parentLinkedComponent: {{fileID: 0}}
  m_enableKerning: 1
  m_enableExtraPadding: 0
  checkPaddingRequired: 0
  m_isRichText: 1
  m_parseCtrlCharacters: 1
  m_isOrthographic: 1
  m_isSelfOverlapping: 0
  m_isNonProportionalFont: 0
  m_setPaddingForBoldItalicText: 1
  m_isOrthographic2: 1
  m_renderMode: 0
  m_geometrySortingOrder: 0
  m_IsTextObjectScaleStatic: 0
  m_VertexBufferAutoSizeReduction: 0
  m_useMaxVisibleDescender: 1
  m_pageToDisplay: 1
  m_margin: {{x: 0, y: 0, z: 0, w: 0}}
  m_isUsingLegacyAnimationComponent: 0
  m_isVolumetricText: 0
  m_hasFontReferences: 0
  m_spriteAnimator: {{fileID: 0}}
  m_frameCounterMax: 0"""

def _button(fid, go_fid):
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {BUTTON_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: 0}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []"""

def _dropdown(fid, go_fid):
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {TMP_DROPDOWN_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: 0}}
  m_Template: {{fileID: 0}}
  m_CaptionText: {{fileID: 0}}
  m_CaptionImage: {{fileID: 0}}
  m_ItemText: {{fileID: 0}}
  m_ItemImage: {{fileID: 0}}
  m_Options: []
  m_Value: 0
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []"""

def _inputfield(fid, go_fid, placeholder=""):
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {TMP_INPUTFIELD_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: 0}}
  m_TextViewport: {{fileID: 0}}
  m_TextComponent: {{fileID: 0}}
  m_Placeholder: {{fileID: 0}}
  m_VerticalScrollbar: {{fileID: 0}}
  m_VerticalScrollbarEventHandler: {{fileID: 0}}
  m_LayoutGroup: {{fileID: 0}}
  m_Text: ""
  m_CharacterLimit: 0
  m_ContentType: 0
  m_LineType: 0
  m_InputType: 0
  m_CharacterValidation: 0
  m_KeyboardType: 0
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
  m_OnEndEdit:
    m_PersistentCalls:
      m_Calls: []
  m_OnSubmit:
    m_PersistentCalls:
      m_Calls: []
  m_OnSelect:
    m_PersistentCalls:
      m_Calls: []
  m_OnDeselect:
    m_PersistentCalls:
      m_Calls: []"""

def _slider(fid, go_fid):
    return f"""--- !u!114 &{fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SLIDER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: 0}}
  m_FillRect: {{fileID: 0}}
  m_HandleRect: {{fileID: 0}}
  m_Direction: 0
  m_MinValue: 0
  m_MaxValue: 100
  m_WholeNumbers: 1
  m_Value: 50
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []"""

# ─────────────────────────────────────────────────────────────────────────────
# Higher-level builders  (append to `blocks`, return important IDs)
# ─────────────────────────────────────────────────────────────────────────────

def make_text_go(blocks, name, text, parent_rt_fid, root_order,
                 amin_x=0, amin_y=0, amax_x=1, amax_y=1,
                 font_size=24, bold=False, center=True):
    """Plain TMP text object. Returns (go_fid, rt_fid, tmp_fid)."""
    go = nid(); rt = nid(); cr = nid(); tmp = nid()
    blocks.append(_go_block(go, name, [rt, cr, tmp]))
    blocks.append(_rt_block(rt, go, [], parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_canvas_renderer(cr, go))
    blocks.append(_tmp_text(tmp, go, text, font_size, bold, center))
    return go, rt, tmp

def make_panel_go(blocks, name, parent_rt_fid, root_order,
                  amin_x=0, amin_y=0, amax_x=1, amax_y=1,
                  r=0.08, g=0.08, b=0.12, a=1, active=True, children_rt=None):
    """Image panel. Returns (go_fid, rt_fid, img_fid)."""
    go = nid(); rt = nid(); img = nid(); cr = nid()
    children_rt = children_rt or []
    blocks.append(_go_block(go, name, [rt, img, cr], active=active))
    blocks.append(_rt_block(rt, go, children_rt, parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_image(img, go, r, g, b, a))
    blocks.append(_canvas_renderer(cr, go))
    return go, rt, img

def make_container_go(blocks, name, parent_rt_fid, root_order,
                      amin_x=0, amin_y=0, amax_x=1, amax_y=1, active=True, children_rt=None):
    """Empty container (no Image). Returns (go_fid, rt_fid)."""
    go = nid(); rt = nid()
    children_rt = children_rt or []
    blocks.append(_go_block(go, name, [rt], active=active))
    blocks.append(_rt_block(rt, go, children_rt, parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    return go, rt

def make_button_go(blocks, name, label, parent_rt_fid, root_order,
                   amin_x=0, amin_y=0, amax_x=1, amax_y=1,
                   r=0.2, g=0.5, b=0.8, a=1):
    """Button with TMP label child. Returns (go_fid, rt_fid, btn_fid, lbl_tmp_fid)."""
    go = nid(); rt = nid(); img = nid(); cr = nid(); btn = nid()
    lbl_go = nid(); lbl_rt = nid(); lbl_cr = nid(); lbl_tmp = nid()

    blocks.append(_go_block(go, name, [rt, img, cr, btn]))
    blocks.append(_rt_block(rt, go, [lbl_rt], parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_image(img, go, r, g, b, a))
    blocks.append(_canvas_renderer(cr, go))
    blocks.append(_button(btn, go))

    blocks.append(_go_block(lbl_go, "Label", [lbl_rt, lbl_cr, lbl_tmp]))
    blocks.append(_rt_block(lbl_rt, lbl_go, [], rt, 0, 0, 0, 1, 1))
    blocks.append(_canvas_renderer(lbl_cr, lbl_go))
    blocks.append(_tmp_text(lbl_tmp, lbl_go, label, 28, bold=True))

    return go, rt, btn, lbl_tmp

def make_dropdown_go(blocks, name, parent_rt_fid, root_order,
                     amin_x=0, amin_y=0, amax_x=0.5, amax_y=1):
    """TMP_Dropdown. Returns (go_fid, rt_fid, dd_fid)."""
    go = nid(); rt = nid(); img = nid(); cr = nid(); dd = nid()
    blocks.append(_go_block(go, name, [rt, img, cr, dd]))
    blocks.append(_rt_block(rt, go, [], parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_image(img, go, 0.15, 0.15, 0.2, 1))
    blocks.append(_canvas_renderer(cr, go))
    blocks.append(_dropdown(dd, go))
    return go, rt, dd

def make_inputfield_go(blocks, name, placeholder, parent_rt_fid, root_order,
                       amin_x=0, amin_y=0, amax_x=1, amax_y=1):
    """TMP_InputField. Returns (go_fid, rt_fid, if_fid)."""
    go = nid(); rt = nid(); img = nid(); cr = nid(); inf = nid()
    blocks.append(_go_block(go, name, [rt, img, cr, inf]))
    blocks.append(_rt_block(rt, go, [], parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_image(img, go, 0.1, 0.1, 0.15, 1))
    blocks.append(_canvas_renderer(cr, go))
    blocks.append(_inputfield(inf, go, placeholder))
    return go, rt, inf

def make_slider_go(blocks, name, parent_rt_fid, root_order,
                   amin_x=0, amin_y=0, amax_x=1, amax_y=1):
    """Slider. Returns (go_fid, rt_fid, slider_fid)."""
    go = nid(); rt = nid(); img = nid(); cr = nid(); sl = nid()
    blocks.append(_go_block(go, name, [rt, img, cr, sl]))
    blocks.append(_rt_block(rt, go, [], parent_rt_fid, root_order,
                            amin_x, amin_y, amax_x, amax_y))
    blocks.append(_image(img, go, 0.15, 0.15, 0.2, 1))
    blocks.append(_canvas_renderer(cr, go))
    blocks.append(_slider(sl, go))
    return go, rt, sl

# ─────────────────────────────────────────────────────────────────────────────
# Canvas root builder
# ─────────────────────────────────────────────────────────────────────────────

def make_canvas_root(blocks, scene_name, script_guid, script_fields_str, child_rt_fids):
    """Canvas root at fixed fileIDs 300000-300005. Returns canvas RT fid (300001)."""
    comps_str = "\n".join(f"  - component: {{fileID: {c}}}" for c in [300001,300002,300003,300004,300005])
    child_str = "\n".join(f"  - {{fileID: {c}}}" for c in child_rt_fids)
    child_block = f"  m_Children:\n{child_str}" if child_rt_fids else "  m_Children: []"

    canvas_go = f"""--- !u!1 &300000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{comps_str}
  m_Layer: 5
  m_Name: {scene_name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1"""

    canvas_rt = f"""--- !u!224 &300001
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 0, y: 0, z: 0}}
  m_ConstrainProportionsScale: 0
{child_block}
  m_Father: {{fileID: 0}}
  m_RootOrder: 2
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 0, y: 0}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0, y: 1}}"""

    canvas_comp = f"""--- !u!223 &300002
Canvas:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_Enabled: 1
  serializedVersion: 3
  m_RenderMode: 0
  m_Camera: {{fileID: 0}}
  m_PlaneDistance: 100
  m_PixelPerfect: 0
  m_ReceivesEvents: 1
  m_OverrideSorting: 0
  m_OverridePixelPerfect: 0
  m_SortingBucketNormalizedSize: 0
  m_AdditionalShaderChannelsFlag: 25
  m_SortingLayerID: 0
  m_SortingOrder: 0
  m_TargetDisplay: 0"""

    scaler = f"""--- !u!114 &300003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {CANVAS_SCALER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_UiScaleMode: 1
  m_ReferencePixelsPerUnit: 100
  m_ScaleFactor: 1
  m_ReferenceResolution: {{x: 1080, y: 1920}}
  m_ScreenMatchMode: 0
  m_MatchWidthOrHeight: 0.5
  m_PhysicalUnit: 3
  m_FallbackScreenDPI: 96
  m_DefaultSpriteDPI: 96
  m_DynamicPixelsPerUnit: 1
  m_PresetInfoIsWorld: 0"""

    raycaster = f"""--- !u!114 &300004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GRAPHIC_RAYCASTER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_IgnoreReversedGraphics: 1
  m_BlockingObjects: 0
  m_BlockingMask:
    serializedVersion: 2
    m_Bits: 4294967295"""

    ui_script = f"""--- !u!114 &300005
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{script_fields_str}"""

    for b in [canvas_go, canvas_rt, canvas_comp, scaler, raycaster, ui_script]:
        blocks.append(b)
    return 300001  # canvas RT fid

# ─────────────────────────────────────────────────────────────────────────────
# Scene assemblers
# ─────────────────────────────────────────────────────────────────────────────

def fid_ref(fid):
    return f"{{fileID: {fid}}}"

def build_scene(blocks):
    return "\n".join([scene_header(), camera_block(), event_system_block()] + blocks)

# ─────────────────────────────────────────────────────────────────────────────
# DASHBOARD
# ─────────────────────────────────────────────────────────────────────────────
def build_dashboard():
    reset()
    blocks = []

    # We need to know child RT fids before building canvas root.
    # Strategy: build children first, collect their RT fids, then build canvas.
    # But canvas root needs child RT fids. Solution: use placeholder, build children
    # starting at 1000000, canvas at 300000-300005 (fixed).
    # Children RT fids are collected and passed to canvas root builder.

    child_rt_fids = []  # to be filled

    # ── Header Panel
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.85, 1, 1, 0.06, 0.1, 0.18)
    child_rt_fids.append(hdr_rt)

    teamName_go, _, teamName_tmp = make_text_go(blocks, "TeamNameText", "FC Equipo",
                                                 hdr_rt, 0, 0.02, 0.6, 0.98, 1,
                                                 font_size=48, bold=True, center=False)
    leaguePos_go, _, leaguePos_tmp = make_text_go(blocks, "LeaguePositionText", "1ª",
                                                    hdr_rt, 1, 0.6, 0.75, 0.98, 1,
                                                    font_size=36, bold=True)
    league_go, _, league_tmp = make_text_go(blocks, "LeagueText", "División",
                                              hdr_rt, 2, 0.75, 0.5, 1, 1,
                                              font_size=24)
    ovr_go, _, ovr_tmp = make_text_go(blocks, "OverallRatingText", "OVR 75",
                                        hdr_rt, 3, 0.75, 0, 1, 0.5,
                                        font_size=24)
    # Update header children RT
    # (We patch the RT block retroactively by rebuilding it)

    # ── Career Bar
    career_go, career_rt, _ = make_panel_go(blocks, "CareerBar", 300001, 1,
                                             0, 0.78, 1, 0.85, 0.05, 0.08, 0.15)
    child_rt_fids.append(career_rt)
    carBal_go, _, carBal_tmp = make_text_go(blocks, "CareerBalanceText", "£0",
                                              career_rt, 0, 0, 0, 0.5, 1, font_size=24)
    prem_go, _, prem_tmp = make_text_go(blocks, "PremiumCoinsText", "0 Monedas",
                                          career_rt, 1, 0.5, 0, 1, 1, font_size=24)

    # ── NextMatch Panel
    nm_go, nm_rt, _ = make_panel_go(blocks, "NextMatchPanel", 300001, 2,
                                     0, 0.68, 1, 0.78, 0.07, 0.12, 0.2)
    child_rt_fids.append(nm_rt)
    nxt_go, _, nxt_tmp = make_text_go(blocks, "NextMatchText", "Próximo: —",
                                        nm_rt, 0, 0, 0, 0.7, 1, font_size=24)
    bud_go, _, bud_tmp = make_text_go(blocks, "BudgetText", "Presupuesto: £0",
                                        nm_rt, 1, 0.7, 0, 1, 1, font_size=24)

    # ── Button Grid
    grid_go, grid_rt, _ = make_panel_go(blocks, "ButtonGrid", 300001, 3,
                                         0.02, 0.18, 0.98, 0.67, 0.06, 0.08, 0.12)
    child_rt_fids.append(grid_rt)

    COLS = [(0, 0.33), (0.33, 0.67), (0.67, 1)]
    ROWS = [(0.66, 1), (0.33, 0.66), (0, 0.33)]
    BTN_DEFS = [
        ("PlayMatchButton",    "⚽ Jugar Partido",  0.07, 0.47, 0.23),
        ("SquadButton",        "👥 Plantilla",      0.18, 0.35, 0.65),
        ("TacticsButton",      "🎯 Tácticas",       0.45, 0.18, 0.65),
        ("MarketButton",       "🔄 Mercado",        0.75, 0.35, 0.12),
        ("FinancesButton",     "💰 Finanzas",       0.10, 0.22, 0.45),
        ("CompetitionsButton", "🏆 Competiciones",  0.75, 0.60, 0.10),
        ("ShopButton",         "🛒 Tienda",         0.72, 0.10, 0.10),
        ("LegendsButton",      "⭐ Leyendas",       0.75, 0.65, 0.00),
        ("ResignButton",       "🚪 Resignar",       0.40, 0.40, 0.40),
    ]
    btn_fids = {}  # name -> (btn_fid)
    for i, (bname, blabel, r, g, b) in enumerate(BTN_DEFS):
        row, col = divmod(i, 3)
        c0, c1 = COLS[col]
        r0, r1 = ROWS[row]
        _, _, btn_fid, _ = make_button_go(blocks, bname, blabel, grid_rt, i,
                                           c0, r0, c1, r1, r, g, b)
        btn_fids[bname] = btn_fid

    # Patch header children
    _patch_rt_children(blocks, hdr_rt, [teamName_go + 1, leaguePos_go + 1, league_go + 1, ovr_go + 1])
    _patch_rt_children(blocks, career_rt, [carBal_go + 1, prem_go + 1])
    _patch_rt_children(blocks, nm_rt, [nxt_go + 1, bud_go + 1])

    # Build script fields
    sf = f"""  teamNameText: {{fileID: {teamName_tmp}}}
  leagueText: {{fileID: {league_tmp}}}
  leaguePositionText: {{fileID: {leaguePos_tmp}}}
  budgetText: {{fileID: {bud_tmp}}}
  overallRatingText: {{fileID: {ovr_tmp}}}
  nextMatchText: {{fileID: {nxt_tmp}}}
  playMatchButton: {{fileID: {btn_fids['PlayMatchButton']}}}
  squadButton: {{fileID: {btn_fids['SquadButton']}}}
  tacticsButton: {{fileID: {btn_fids['TacticsButton']}}}
  marketButton: {{fileID: {btn_fids['MarketButton']}}}
  financesButton: {{fileID: {btn_fids['FinancesButton']}}}
  competitionsButton: {{fileID: {btn_fids['CompetitionsButton']}}}
  shopButton: {{fileID: {btn_fids['ShopButton']}}}
  legendsButton: {{fileID: {btn_fids['LegendsButton']}}}
  resignButton: {{fileID: {btn_fids['ResignButton']}}}
  careerBalanceText: {{fileID: {carBal_tmp}}}
  premiumCoinsText: {{fileID: {prem_tmp}}}"""

    make_canvas_root(blocks, "Dashboard", DashboardUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# Helper to patch RT children (we insert after building)
# ─────────────────────────────────────────────────────────────────────────────
def _patch_rt_children(blocks, rt_fid, child_rt_fids):
    """Find and update the RectTransform block for rt_fid with the given children."""
    target = f"--- !u!224 &{rt_fid}\n"
    for i, b in enumerate(blocks):
        if b.startswith(target):
            # Replace m_Children line
            if not child_rt_fids:
                new_ch = "  m_Children: []"
            else:
                lines = "\n".join(f"  - {{fileID: {c}}}" for c in child_rt_fids)
                new_ch = f"  m_Children:\n{lines}"
            # Replace old children block
            import re
            b = re.sub(r"  m_Children:.*?(\n  m_Father:)", new_ch + r"\1", b, flags=re.DOTALL)
            blocks[i] = b
            return

# ─────────────────────────────────────────────────────────────────────────────
# SQUAD
# ─────────────────────────────────────────────────────────────────────────────
def build_squad():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.9, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Plantilla",
                                    hdr_rt, 0, 0, 0, 0.6, 1, font_size=40, bold=True)
    _, _, pos_dd = make_dropdown_go(blocks, "PositionFilterDropdown",
                                    hdr_rt, 1, 0.6, 0.1, 0.85, 0.9)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     hdr_rt, 2, 0.85, 0.1, 1, 0.9,
                                     0.4, 0.1, 0.1)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, pos_dd-3, back_btn-3])

    # Left panel (list)
    left_go, left_rt, _ = make_panel_go(blocks, "ListPanel", 300001, 1,
                                         0, 0, 0.6, 0.9, 0.06, 0.08, 0.12)
    child_rt_fids.append(left_rt)
    cont_go, cont_rt = make_container_go(blocks, "PlayerListContainer",
                                          left_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, left_rt, [cont_rt])

    # Right panel (detail)
    right_go, right_rt, _ = make_panel_go(blocks, "DetailPanel", 300001, 2,
                                            0.6, 0, 1, 0.9, 0.08, 0.1, 0.15)
    child_rt_fids.append(right_rt)
    dname_go, _, dname_tmp = make_text_go(blocks, "PlayerDetailName", "—",
                                            right_rt, 0, 0, 0.7, 1, 1,
                                            font_size=32, bold=True)
    dstats_go, _, dstats_tmp = make_text_go(blocks, "PlayerDetailStats", "Selecciona un jugador",
                                              right_rt, 1, 0, 0.3, 1, 0.7,
                                              font_size=22)
    dcontract_go, _, dcontract_tmp = make_text_go(blocks, "PlayerDetailContract", "",
                                                    right_rt, 2, 0, 0, 1, 0.3,
                                                    font_size=20)
    _patch_rt_children(blocks, right_rt, [dname_go+1, dstats_go+1, dcontract_go+1])

    sf = f"""  playerListContainer: {{fileID: {cont_rt}}}
  playerRowPrefab: {{fileID: 0}}
  playerDetailName: {{fileID: {dname_tmp}}}
  playerDetailStats: {{fileID: {dstats_tmp}}}
  playerDetailContract: {{fileID: {dcontract_tmp}}}
  positionFilterDropdown: {{fileID: {pos_dd}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "Squad", SquadUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# TRANSFER MARKET
# ─────────────────────────────────────────────────────────────────────────────
def build_transfer_market():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Mercado",
                                    hdr_rt, 0, 0, 0, 0.4, 1, font_size=40, bold=True)
    bal_go, _, bal_tmp = make_text_go(blocks, "BalanceText", "£0",
                                        hdr_rt, 1, 0.4, 0, 0.75, 1, font_size=28)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     hdr_rt, 2, 0.78, 0.1, 1, 0.9, 0.4, 0.1, 0.1)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, bal_go+1, back_btn-3])

    # Tab row
    tab_go, tab_rt, _ = make_panel_go(blocks, "TabRow", 300001, 1,
                                       0, 0.84, 1, 0.91, 0.08, 0.10, 0.14)
    child_rt_fids.append(tab_rt)
    _, _, buy_btn, _ = make_button_go(blocks, "BuyTabButton", "Comprar",
                                    tab_rt, 0, 0, 0.05, 0.5, 0.95, 0.07, 0.47, 0.23)
    _, _, sell_btn, _ = make_button_go(blocks, "SellTabButton", "Vender",
                                     tab_rt, 1, 0.5, 0.05, 1, 0.95, 0.75, 0.35, 0.12)

    # Filter row
    flt_go, flt_rt, _ = make_panel_go(blocks, "FilterRow", 300001, 2,
                                       0, 0.76, 1, 0.84, 0.08, 0.10, 0.14)
    child_rt_fids.append(flt_rt)
    _, _, pos_dd = make_dropdown_go(blocks, "PositionFilterDropdown",
                                    flt_rt, 0, 0, 0.05, 0.3, 0.95)
    _, _, max_price_if = make_inputfield_go(blocks, "MaxPriceInput", "Precio máx",
                                              flt_rt, 1, 0.3, 0.05, 0.55, 0.95)
    _, _, apply_btn, _ = make_button_go(blocks, "ApplyFilterButton", "Filtrar",
                                      flt_rt, 2, 0.56, 0.05, 0.78, 0.95, 0.07, 0.47, 0.23)
    _, _, clear_btn, _ = make_button_go(blocks, "ClearFilterButton", "Limpiar",
                                      flt_rt, 3, 0.79, 0.05, 1, 0.95, 0.5, 0.3, 0.1)

    # List area
    list_go, list_rt, _ = make_panel_go(blocks, "ListArea", 300001, 3,
                                         0, 0.08, 1, 0.76, 0.06, 0.07, 0.1)
    child_rt_fids.append(list_rt)
    plcont_go, plcont_rt = make_container_go(blocks, "PlayerListContainer",
                                              list_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, list_rt, [plcont_rt])

    # Result message
    res_go, res_rt, _ = make_panel_go(blocks, "ResultRow", 300001, 4,
                                       0, 0, 1, 0.08, 0.06, 0.07, 0.1)
    child_rt_fids.append(res_rt)
    res_msg_go, _, res_tmp = make_text_go(blocks, "ResultMessage", "",
                                            res_rt, 0, 0, 0, 1, 1, font_size=24)
    _patch_rt_children(blocks, res_rt, [res_msg_go+1])

    # Negotiation Panel (hidden overlay)
    neg_go, neg_rt, _ = make_panel_go(blocks, "NegotiationPanel", 300001, 5,
                                       0.1, 0.3, 0.9, 0.7, 0.05, 0.08, 0.12, active=False)
    child_rt_fids.append(neg_rt)
    neg_txt_go, _, neg_tmp = make_text_go(blocks, "NegotiationText", "Oferta por jugador",
                                            neg_rt, 0, 0, 0.6, 1, 1, font_size=28, bold=True)
    _, _, bid_if = make_inputfield_go(blocks, "BidInput", "Oferta (£)",
                                       neg_rt, 1, 0, 0.3, 1, 0.6)
    _, _, conf_bid, _ = make_button_go(blocks, "ConfirmBidButton", "Confirmar oferta",
                                     neg_rt, 2, 0, 0.05, 0.5, 0.3, 0.07, 0.47, 0.23)
    _, _, canc_bid, _ = make_button_go(blocks, "CancelBidButton", "Cancelar",
                                     neg_rt, 3, 0.5, 0.05, 1, 0.3, 0.5, 0.1, 0.1)

    # Sell Panel (hidden overlay)
    sell_pnl_go, sell_pnl_rt, _ = make_panel_go(blocks, "SellPanel", 300001, 6,
                                                  0.1, 0.3, 0.9, 0.7, 0.05, 0.08, 0.12, active=False)
    child_rt_fids.append(sell_pnl_rt)
    sell_txt_go, _, sell_det_tmp = make_text_go(blocks, "SellDetailText", "Vender jugador",
                                                  sell_pnl_rt, 0, 0, 0.5, 1, 1, font_size=28, bold=True)
    _, _, conf_sell, _ = make_button_go(blocks, "ConfirmSellButton", "Confirmar venta",
                                      sell_pnl_rt, 1, 0, 0.05, 0.5, 0.5, 0.07, 0.47, 0.23)
    _, _, canc_sell, _ = make_button_go(blocks, "CancelSellButton", "Cancelar",
                                      sell_pnl_rt, 2, 0.5, 0.05, 1, 0.5, 0.5, 0.1, 0.1)

    sf = f"""  buyTabButton: {{fileID: {buy_btn}}}
  sellTabButton: {{fileID: {sell_btn}}}
  playerListContainer: {{fileID: {plcont_rt}}}
  playerRowPrefab: {{fileID: 0}}
  positionFilterDropdown: {{fileID: {pos_dd}}}
  maxPriceInput: {{fileID: {max_price_if}}}
  applyFilterButton: {{fileID: {apply_btn}}}
  clearFilterButton: {{fileID: {clear_btn}}}
  negotiationPanel: {{fileID: {neg_go}}}
  negotiationText: {{fileID: {neg_tmp}}}
  bidInput: {{fileID: {bid_if}}}
  confirmBidButton: {{fileID: {conf_bid}}}
  cancelBidButton: {{fileID: {canc_bid}}}
  sellPanel: {{fileID: {sell_pnl_go}}}
  sellDetailText: {{fileID: {sell_det_tmp}}}
  confirmSellButton: {{fileID: {conf_sell}}}
  cancelSellButton: {{fileID: {canc_sell}}}
  balanceText: {{fileID: {bal_tmp}}}
  resultMessage: {{fileID: {res_tmp}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "TransferMarket", TransferMarketUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# SHOP
# ─────────────────────────────────────────────────────────────────────────────
def build_shop():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Tienda",
                                    hdr_rt, 0, 0, 0, 0.5, 1, font_size=40, bold=True)
    bal_go, _, bal_tmp = make_text_go(blocks, "BalanceText", "£0 | 0 FutCoins",
                                        hdr_rt, 1, 0.5, 0, 1, 1, font_size=24)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, bal_go+1])

    # Tab row
    tab_go, tab_rt, _ = make_panel_go(blocks, "TabRow", 300001, 1,
                                       0, 0.83, 1, 0.91, 0.08, 0.10, 0.14)
    child_rt_fids.append(tab_rt)
    _, _, coins_btn, _ = make_button_go(blocks, "TabCoinsButton", "FutCoins",
                                      tab_rt, 0, 0, 0.05, 0.33, 0.95, 0.75, 0.60, 0.10)
    _, _, curr_btn, _ = make_button_go(blocks, "TabCurrencyButton", "Divisa",
                                     tab_rt, 1, 0.34, 0.05, 0.66, 0.95, 0.10, 0.45, 0.72)
    _, _, packs_btn, _ = make_button_go(blocks, "TabPacksButton", "Sobres",
                                      tab_rt, 2, 0.67, 0.05, 1, 0.95, 0.55, 0.27, 0.07)

    # Exchange rate
    xrate_go, xrate_rt, _ = make_panel_go(blocks, "ExchangeRateRow", 300001, 2,
                                           0, 0.77, 1, 0.83, 0.06, 0.07, 0.1)
    child_rt_fids.append(xrate_rt)
    xr_go, _, xr_tmp = make_text_go(blocks, "ExchangeRateText", "1 £ = 100 FutCoins",
                                      xrate_rt, 0, 0, 0, 1, 1, font_size=22)
    _patch_rt_children(blocks, xrate_rt, [xr_go+1])

    # Item container
    item_go, item_rt, _ = make_panel_go(blocks, "ItemArea", 300001, 3,
                                         0, 0.1, 1, 0.77, 0.06, 0.07, 0.1)
    child_rt_fids.append(item_rt)
    icont_go, icont_rt = make_container_go(blocks, "ItemContainer",
                                            item_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, item_rt, [icont_rt])

    # Feedback + back
    foot_go, foot_rt, _ = make_panel_go(blocks, "FooterRow", 300001, 4,
                                         0, 0, 1, 0.1, 0.06, 0.07, 0.1)
    child_rt_fids.append(foot_rt)
    fb_go, _, fb_tmp = make_text_go(blocks, "FeedbackText", "",
                                     foot_rt, 0, 0.15, 0, 0.85, 1, font_size=22)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     foot_rt, 1, 0, 0.05, 0.15, 0.95, 0.4, 0.1, 0.1)
    _patch_rt_children(blocks, foot_rt, [fb_go+1, back_btn-3])

    sf = f"""  tabCoinsButton: {{fileID: {coins_btn}}}
  tabCurrencyButton: {{fileID: {curr_btn}}}
  tabPacksButton: {{fileID: {packs_btn}}}
  itemContainer: {{fileID: {icont_rt}}}
  itemCardPrefab: {{fileID: 0}}
  feedbackText: {{fileID: {fb_tmp}}}
  balanceText: {{fileID: {bal_tmp}}}
  exchangeRateText: {{fileID: {xr_tmp}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "Shop", ShopUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# TACTICS
# ─────────────────────────────────────────────────────────────────────────────
def build_tactics():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Tácticas",
                                    hdr_rt, 0, 0, 0, 0.6, 1, font_size=40, bold=True)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     hdr_rt, 1, 0.8, 0.1, 1, 0.9, 0.4, 0.1, 0.1)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, back_btn-3])

    # Formation row
    form_go, form_rt, _ = make_panel_go(blocks, "FormationRow", 300001, 1,
                                         0, 0.84, 1, 0.91, 0.06, 0.07, 0.1)
    child_rt_fids.append(form_rt)
    _, _, form_dd = make_dropdown_go(blocks, "FormationDropdown",
                                     form_rt, 0, 0.1, 0.1, 0.9, 0.9)

    # Pitch area
    pitch_go, pitch_rt, _ = make_panel_go(blocks, "PitchArea", 300001, 2,
                                           0.05, 0.38, 0.95, 0.84, 0.09, 0.35, 0.09)
    child_rt_fids.append(pitch_rt)
    pcont_go, pcont_rt = make_container_go(blocks, "PitchContainer",
                                            pitch_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, pitch_rt, [pcont_rt])

    # Sliders panel
    sliders_go, sliders_rt, _ = make_panel_go(blocks, "SlidersPanel", 300001, 3,
                                               0, 0.08, 1, 0.38, 0.06, 0.07, 0.1)
    child_rt_fids.append(sliders_rt)
    SLIDERS = [
        ("PressingSlider",      "Pressing"),
        ("TempoSlider",         "Tempo"),
        ("WidthSlider",         "Amplitud"),
        ("DefensiveLineSlider", "Línea Def"),
    ]
    slider_fids = {}
    for idx, (sname, slabel) in enumerate(SLIDERS):
        row_y0 = 1 - (idx + 1) * 0.25
        row_y1 = 1 - idx * 0.25
        row_go, row_rt, _ = make_panel_go(blocks, f"{sname}Row", sliders_rt, idx,
                                           0, row_y0, 1, row_y1, 0.06, 0.07, 0.10)
        lbl_go, _, _ = make_text_go(blocks, f"{sname}Label", slabel,
                                      row_rt, 0, 0, 0, 0.3, 1, font_size=22)
        _, _, sl_fid = make_slider_go(blocks, sname, row_rt, 1, 0.3, 0, 1, 1)
        slider_fids[sname] = sl_fid

    # Prediction panel (hidden)
    pred_go, pred_rt, _ = make_panel_go(blocks, "PredictionPanel", 300001, 4,
                                         0.05, 0.2, 0.95, 0.8, 0.05, 0.08, 0.12, active=False)
    child_rt_fids.append(pred_rt)
    pred_txt_go, _, pred_tmp = make_text_go(blocks, "PredictionText", "",
                                              pred_rt, 0, 0, 0, 1, 1, font_size=28)
    _patch_rt_children(blocks, pred_rt, [pred_txt_go+1])

    sf = f"""  formationDropdown: {{fileID: {form_dd}}}
  pitchContainer: {{fileID: {pcont_rt}}}
  playerDotPrefab: {{fileID: 0}}
  pressingSlider: {{fileID: {slider_fids['PressingSlider']}}}
  tempoSlider: {{fileID: {slider_fids['TempoSlider']}}}
  widthSlider: {{fileID: {slider_fids['WidthSlider']}}}
  defensiveLineSlider: {{fileID: {slider_fids['DefensiveLineSlider']}}}
  predictionText: {{fileID: {pred_tmp}}}
  predictionPanel: {{fileID: {pred_go}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "Tactics", TacticsUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# FINANCES
# ─────────────────────────────────────────────────────────────────────────────
def build_finances():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Finanzas",
                                    hdr_rt, 0, 0, 0, 0.7, 1, font_size=40, bold=True)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     hdr_rt, 1, 0.78, 0.1, 1, 0.9, 0.4, 0.1, 0.1)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, back_btn-3])

    # Summary grid
    summ_go, summ_rt, _ = make_panel_go(blocks, "SummaryPanel", 300001, 1,
                                         0, 0.68, 1, 0.91, 0.06, 0.07, 0.12)
    child_rt_fids.append(summ_rt)
    LABELS = [
        ("BalanceLabel",        "Balance:          £0",    0, 0.75, 0.5, 1),
        ("TransferBudgetLabel", "Transfer Budget:  £0",    0.5, 0.75, 1, 1),
        ("WageBudgetLabel",     "Wage Budget:      £0/wk", 0, 0.5, 0.5, 0.75),
        ("MonthlyIncomeLabel",  "Monthly Income:   £0",    0.5, 0.5, 1, 0.75),
        ("MonthlyExpenseLabel", "Monthly Expenses: £0",    0, 0.25, 0.5, 0.5),
    ]
    lbl_fids = {}
    for idx, (lname, ltxt, x0, y0, x1, y1) in enumerate(LABELS):
        lg, _, lt = make_text_go(blocks, lname, ltxt,
                                   summ_rt, idx, x0, y0, x1, y1, font_size=22)
        lbl_fids[lname] = lt

    # Budget allocation
    bud_row_go, bud_row_rt, _ = make_panel_go(blocks, "BudgetAllocationRow", 300001, 2,
                                               0, 0.56, 1, 0.68, 0.06, 0.07, 0.1)
    child_rt_fids.append(bud_row_rt)
    bud_go, _, bud_tmp = make_text_go(blocks, "BudgetAllocationText", "",
                                        bud_row_rt, 0, 0, 0, 1, 1, font_size=22)
    _patch_rt_children(blocks, bud_row_rt, [bud_go+1])

    # History area
    hist_go, hist_rt, _ = make_panel_go(blocks, "HistoryArea", 300001, 3,
                                         0, 0, 1, 0.56, 0.06, 0.07, 0.1)
    child_rt_fids.append(hist_rt)
    hcont_go, hcont_rt = make_container_go(blocks, "HistoryContainer",
                                            hist_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, hist_rt, [hcont_rt])

    sf = f"""  balanceLabel: {{fileID: {lbl_fids['BalanceLabel']}}}
  transferBudgetLabel: {{fileID: {lbl_fids['TransferBudgetLabel']}}}
  wageBudgetLabel: {{fileID: {lbl_fids['WageBudgetLabel']}}}
  monthlyIncomeLabel: {{fileID: {lbl_fids['MonthlyIncomeLabel']}}}
  monthlyExpenseLabel: {{fileID: {lbl_fids['MonthlyExpenseLabel']}}}
  historyContainer: {{fileID: {hcont_rt}}}
  historyRowPrefab: {{fileID: 0}}
  budgetAllocationText: {{fileID: {bud_tmp}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "Finances", FinancesUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# COMPETITIONS
# ─────────────────────────────────────────────────────────────────────────────
def build_competitions():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Competiciones",
                                    hdr_rt, 0, 0, 0, 0.4, 1, font_size=36, bold=True)
    _, _, league_dd = make_dropdown_go(blocks, "LeagueDropdown",
                                       hdr_rt, 1, 0.4, 0.1, 0.78, 0.9)
    _, _, back_btn, _ = make_button_go(blocks, "BackButton", "← Atrás",
                                     hdr_rt, 2, 0.79, 0.1, 1, 0.9, 0.4, 0.1, 0.1)
    _patch_rt_children(blocks, hdr_rt, [title_go+1, league_dd-3, back_btn-3])

    # Tab row
    tab_go, tab_rt, _ = make_panel_go(blocks, "TabRow", 300001, 1,
                                       0, 0.84, 1, 0.91, 0.08, 0.10, 0.14)
    child_rt_fids.append(tab_rt)
    _, _, tab_lg, _ = make_button_go(blocks, "TabLeagueButton", "Liga",
                                   tab_rt, 0, 0, 0.05, 0.33, 0.95, 0.10, 0.22, 0.45)
    _, _, tab_cp, _ = make_button_go(blocks, "TabCupsButton", "Copas",
                                   tab_rt, 1, 0.34, 0.05, 0.66, 0.95, 0.45, 0.18, 0.65)
    _, _, tab_tr, _ = make_button_go(blocks, "TabTrophiesButton", "Trofeos",
                                   tab_rt, 2, 0.67, 0.05, 1, 0.95, 0.75, 0.60, 0.10)

    # League info row
    info_go, info_rt, _ = make_panel_go(blocks, "LeagueInfoRow", 300001, 2,
                                         0, 0.76, 1, 0.84, 0.06, 0.07, 0.1)
    child_rt_fids.append(info_rt)
    lgname_go, _, lgname_tmp = make_text_go(blocks, "LeagueNameText", "Liga",
                                              info_rt, 0, 0, 0, 0.5, 1, font_size=28, bold=True)
    mday_go, _, mday_tmp = make_text_go(blocks, "MatchdayText", "Jornada 1",
                                          info_rt, 1, 0.5, 0, 1, 1, font_size=24)
    _patch_rt_children(blocks, info_rt, [lgname_go+1, mday_go+1])

    # League panel (table container)
    lg_pnl_go, lg_pnl_rt, _ = make_panel_go(blocks, "LeaguePanel", 300001, 3,
                                              0, 0, 1, 0.76, 0.06, 0.07, 0.1)
    child_rt_fids.append(lg_pnl_rt)
    tbl_go, tbl_rt = make_container_go(blocks, "TableContainer", lg_pnl_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, lg_pnl_rt, [tbl_rt])

    # Cups panel (hidden)
    cups_go, cups_rt, _ = make_panel_go(blocks, "CupsPanel", 300001, 4,
                                         0, 0, 1, 0.76, 0.06, 0.07, 0.1, active=False)
    child_rt_fids.append(cups_rt)
    cup_cont_go, cup_cont_rt = make_container_go(blocks, "CupListContainer",
                                                   cups_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, cups_rt, [cup_cont_rt])

    # Trophies panel (hidden)
    tr_go, tr_rt, _ = make_panel_go(blocks, "TrophiesPanel", 300001, 5,
                                     0, 0, 1, 0.76, 0.06, 0.07, 0.1, active=False)
    child_rt_fids.append(tr_rt)
    _, _, trophy_if = make_inputfield_go(blocks, "TrophyTeamInput", "Equipo...",
                                          tr_rt, 0, 0, 0.88, 0.6, 1)
    _, _, trophy_search, _ = make_button_go(blocks, "TrophySearchButton", "Buscar",
                                          tr_rt, 1, 0.61, 0.88, 1, 1, 0.10, 0.22, 0.45)
    tr_cont_go, tr_cont_rt = make_container_go(blocks, "TrophyContainer",
                                                tr_rt, 2, 0, 0, 1, 0.88)
    _patch_rt_children(blocks, tr_rt, [trophy_if-3, trophy_search-3, tr_cont_rt])

    sf = f"""  leagueDropdown: {{fileID: {league_dd}}}
  leagueNameText: {{fileID: {lgname_tmp}}}
  matchdayText: {{fileID: {mday_tmp}}}
  tableContainer: {{fileID: {tbl_rt}}}
  tableRowPrefab: {{fileID: 0}}
  cupsPanel: {{fileID: {cups_go}}}
  cupListContainer: {{fileID: {cup_cont_rt}}}
  cupRowPrefab: {{fileID: 0}}
  trophiesPanel: {{fileID: {tr_go}}}
  trophyContainer: {{fileID: {tr_cont_rt}}}
  trophyRowPrefab: {{fileID: 0}}
  trophyTeamInput: {{fileID: {trophy_if}}}
  trophySearchButton: {{fileID: {trophy_search}}}
  tabLeagueButton: {{fileID: {tab_lg}}}
  tabCupsButton: {{fileID: {tab_cp}}}
  tabTrophiesButton: {{fileID: {tab_tr}}}
  backButton: {{fileID: {back_btn}}}"""

    make_canvas_root(blocks, "Competitions", CompetitionsUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# TEAM SELECTION
# ─────────────────────────────────────────────────────────────────────────────
def build_team_selection():
    reset()
    blocks = []
    child_rt_fids = []

    # Header
    hdr_go, hdr_rt, _ = make_panel_go(blocks, "HeaderPanel", 300001, 0,
                                       0, 0.91, 1, 1, 0.06, 0.08, 0.15)
    child_rt_fids.append(hdr_rt)
    title_go, _, _ = make_text_go(blocks, "TitleText", "Selección de Equipo",
                                    hdr_rt, 0, 0, 0, 1, 1, font_size=36, bold=True)

    # League row
    lg_row_go, lg_row_rt, _ = make_panel_go(blocks, "LeagueRow", 300001, 1,
                                             0, 0.83, 1, 0.91, 0.06, 0.07, 0.1)
    child_rt_fids.append(lg_row_rt)
    _, _, lg_dd = make_dropdown_go(blocks, "LeagueDropdown",
                                   lg_row_rt, 0, 0, 0.05, 0.5, 0.95)
    lg_info_go, _, lg_info_tmp = make_text_go(blocks, "LeagueInfoText", "Selecciona una liga",
                                               lg_row_rt, 1, 0.5, 0, 1, 1, font_size=22)
    _patch_rt_children(blocks, lg_row_rt, [lg_dd-3, lg_info_go+1])

    # Left: team list
    left_go, left_rt, _ = make_panel_go(blocks, "TeamListPanel", 300001, 2,
                                         0, 0.1, 0.5, 0.83, 0.06, 0.07, 0.1)
    child_rt_fids.append(left_rt)
    tcont_go, tcont_rt = make_container_go(blocks, "TeamListContainer",
                                            left_rt, 0, 0, 0, 1, 1)
    _patch_rt_children(blocks, left_rt, [tcont_rt])

    # Right: team info panel
    right_go, right_rt, _ = make_panel_go(blocks, "TeamInfoPanel", 300001, 3,
                                            0.5, 0.1, 1, 0.83, 0.08, 0.10, 0.15, active=False)
    child_rt_fids.append(right_rt)
    INFO_LABELS = [
        ("TeamNameText",     "Equipo",       0, 0.83, 1, 1),
        ("TeamBudgetText",   "Presupuesto:", 0, 0.66, 1, 0.83),
        ("TeamCurrencyText", "Divisa:",      0, 0.5,  1, 0.66),
        ("TeamBalanceText",  "Balance:",     0, 0.33, 1, 0.5),
        ("TeamStadiumText",  "Estadio:",     0, 0.16, 1, 0.33),
        ("PreviousClubsText","Clubes ant.:", 0, 0,    1, 0.16),
    ]
    info_fids = {}
    for idx, (lname, ltxt, x0, y0, x1, y1) in enumerate(INFO_LABELS):
        lg, _, lt = make_text_go(blocks, lname, ltxt,
                                   right_rt, idx, x0, y0, x1, y1, font_size=24)
        info_fids[lname] = lt

    # Bottom row
    bot_go, bot_rt, _ = make_panel_go(blocks, "BottomRow", 300001, 4,
                                       0, 0, 1, 0.1, 0.06, 0.07, 0.1)
    child_rt_fids.append(bot_rt)
    _, _, conf_btn, _ = make_button_go(blocks, "ConfirmButton", "Confirmar",
                                     bot_rt, 0, 0, 0.05, 0.3, 0.95, 0.07, 0.47, 0.23)
    _, _, bk_btn, _ = make_button_go(blocks, "BackButton", "Atrás",
                                    bot_rt, 1, 0.31, 0.05, 0.55, 0.95, 0.4, 0.1, 0.1)
    fb_go, _, fb_tmp = make_text_go(blocks, "FeedbackText", "",
                                      bot_rt, 2, 0.56, 0, 1, 1, font_size=22)
    _patch_rt_children(blocks, bot_rt, [conf_btn-3, bk_btn-3, fb_go+1])

    sf = f"""  leagueDropdown: {{fileID: {lg_dd}}}
  leagueInfoText: {{fileID: {lg_info_tmp}}}
  teamListContainer: {{fileID: {tcont_rt}}}
  teamButtonPrefab: {{fileID: 0}}
  teamInfoPanel: {{fileID: {right_go}}}
  teamNameText: {{fileID: {info_fids['TeamNameText']}}}
  teamBudgetText: {{fileID: {info_fids['TeamBudgetText']}}}
  teamCurrencyText: {{fileID: {info_fids['TeamCurrencyText']}}}
  teamBalanceText: {{fileID: {info_fids['TeamBalanceText']}}}
  teamStadiumText: {{fileID: {info_fids['TeamStadiumText']}}}
  previousClubsText: {{fileID: {info_fids['PreviousClubsText']}}}
  confirmButton: {{fileID: {conf_btn}}}
  backButton: {{fileID: {bk_btn}}}
  feedbackText: {{fileID: {fb_tmp}}}"""

    make_canvas_root(blocks, "TeamSelection", TeamSelectionUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# MATCH
# ─────────────────────────────────────────────────────────────────────────────
def build_match():
    reset()
    blocks = []
    child_rt_fids = []

    # ── Preview Panel (active at start)
    prev_go, prev_rt, _ = make_panel_go(blocks, "PreviewPanel", 300001, 0,
                                         0, 0, 1, 1, 0.05, 0.08, 0.12)
    child_rt_fids.append(prev_rt)

    home_go, _, home_tmp = make_text_go(blocks, "HomeTeamLabel", "Casa",
                                          prev_rt, 0, 0.05, 0.75, 0.45, 0.95,
                                          font_size=40, bold=True, center=False)
    away_go, _, away_tmp = make_text_go(blocks, "AwayTeamLabel", "Visitante",
                                          prev_rt, 1, 0.55, 0.75, 0.95, 0.95,
                                          font_size=40, bold=True)
    vs_go, _, _ = make_text_go(blocks, "VsLabel", "VS",
                                  prev_rt, 2, 0.4, 0.78, 0.6, 0.92,
                                  font_size=36, bold=True)
    date_go, _, date_tmp = make_text_go(blocks, "MatchDateLabel", "Fecha",
                                          prev_rt, 3, 0.1, 0.65, 0.9, 0.78,
                                          font_size=28)
    # Speed buttons
    _, _, spd3_btn, _ = make_button_go(blocks, "Speed3MinButton", "3 min",
                                     prev_rt, 4, 0.05, 0.5, 0.3, 0.63, 0.18, 0.35, 0.65)
    _, _, spd4_btn, _ = make_button_go(blocks, "Speed4MinButton", "4 min",
                                     prev_rt, 5, 0.38, 0.5, 0.62, 0.63, 0.10, 0.45, 0.72)
    _, _, spd6_btn, _ = make_button_go(blocks, "Speed6MinButton", "6 min",
                                     prev_rt, 6, 0.70, 0.5, 0.95, 0.63, 0.55, 0.27, 0.07)
    spd_ind_go, _, spd_ind_tmp = make_text_go(blocks, "SpeedIndicatorText", "Velocidad: 4 min",
                                                prev_rt, 7, 0.1, 0.43, 0.9, 0.5,
                                                font_size=22)
    _, _, sim_btn, _ = make_button_go(blocks, "SimulateButton", "▶ Simular",
                                    prev_rt, 8, 0.2, 0.3, 0.8, 0.43, 0.07, 0.47, 0.23)

    # ── Live Panel (hidden at start)
    live_go, live_rt, _ = make_panel_go(blocks, "LivePanel", 300001, 1,
                                         0, 0, 1, 1, 0.05, 0.08, 0.12, active=False)
    child_rt_fids.append(live_rt)
    score_go, _, score_tmp = make_text_go(blocks, "ScoreLabel", "0 - 0",
                                            live_rt, 0, 0.1, 0.82, 0.9, 0.95,
                                            font_size=72, bold=True)
    min_go, _, min_tmp = make_text_go(blocks, "MinuteLabel", "0'",
                                        live_rt, 1, 0.4, 0.75, 0.6, 0.82,
                                        font_size=32)
    poss_go, _, poss_tmp = make_text_go(blocks, "PossessionLabel", "Pos: 50%",
                                          live_rt, 2, 0, 0.68, 0.25, 0.75,
                                          font_size=22)
    xg_go, _, xg_tmp = make_text_go(blocks, "XGLabel", "xG: 0.0",
                                      live_rt, 3, 0.25, 0.68, 0.5, 0.75,
                                      font_size=22)
    shots_go, _, shots_tmp = make_text_go(blocks, "ShotsLabel", "Tiros: 0",
                                            live_rt, 4, 0.5, 0.68, 0.75, 0.75,
                                            font_size=22)
    mnadv_go, _, mnadv_tmp = make_text_go(blocks, "ManAdvantageLabel", "",
                                            live_rt, 5, 0.75, 0.68, 1, 0.75,
                                            font_size=22)
    evfeed_go, evfeed_rt = make_container_go(blocks, "EventFeed",
                                              live_rt, 6, 0, 0.1, 1, 0.68)
    _, _, ret_btn, _ = make_button_go(blocks, "ReturnButton", "Volver al Dashboard",
                                    live_rt, 7, 0.1, 0.01, 0.9, 0.09, 0.10, 0.22, 0.45)

    # ── Summary Panel (hidden)
    sum_go, sum_rt, _ = make_panel_go(blocks, "SummaryPanel", 300001, 2,
                                       0, 0, 1, 1, 0.05, 0.08, 0.12, active=False)
    child_rt_fids.append(sum_rt)
    sum_txt_go, _, sum_tmp = make_text_go(blocks, "SummaryText", "",
                                            sum_rt, 0, 0.05, 0.1, 0.95, 0.9,
                                            font_size=28)
    _patch_rt_children(blocks, sum_rt, [sum_txt_go+1])

    sf = f"""  previewPanel: {{fileID: {prev_go}}}
  homeTeamLabel: {{fileID: {home_tmp}}}
  awayTeamLabel: {{fileID: {away_tmp}}}
  matchDateLabel: {{fileID: {date_tmp}}}
  speed3MinButton: {{fileID: {spd3_btn}}}
  speed4MinButton: {{fileID: {spd4_btn}}}
  speed6MinButton: {{fileID: {spd6_btn}}}
  speedIndicatorText: {{fileID: {spd_ind_tmp}}}
  livePanel: {{fileID: {live_go}}}
  scoreLabel: {{fileID: {score_tmp}}}
  minuteLabel: {{fileID: {min_tmp}}}
  eventFeed: {{fileID: {evfeed_rt}}}
  eventRowPrefab: {{fileID: 0}}
  manAdvantageLabel: {{fileID: {mnadv_tmp}}}
  possessionLabel: {{fileID: {poss_tmp}}}
  xGLabel: {{fileID: {xg_tmp}}}
  shotsLabel: {{fileID: {shots_tmp}}}
  summaryPanel: {{fileID: {sum_go}}}
  summaryText: {{fileID: {sum_tmp}}}
  simulateButton: {{fileID: {sim_btn}}}
  returnButton: {{fileID: {ret_btn}}}"""

    make_canvas_root(blocks, "Match", MatchDayUI_GUID, sf, child_rt_fids)
    return build_scene(blocks)

# ─────────────────────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────────────────────
SCENES = {
    "Dashboard":     build_dashboard,
    "Squad":         build_squad,
    "TransferMarket":build_transfer_market,
    "Shop":          build_shop,
    "Tactics":       build_tactics,
    "Finances":      build_finances,
    "Competitions":  build_competitions,
    "TeamSelection": build_team_selection,
    "Match":         build_match,
}

for name, builder in SCENES.items():
    path = os.path.join(SCENES_DIR, f"{name}.unity")
    content = builder()
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Written {path}  ({len(content):,} bytes)")

print("\nValidation:")
import re
for name in SCENES:
    path = os.path.join(SCENES_DIR, f"{name}.unity")
    with open(path) as f:
        content = f.read()
    gos = re.findall(r'  m_Name: (.+)', content)
    btns = [n.strip() for n in gos if any(k in n for k in ('Button','Btn','btn'))]
    print(f"  {name}: {len(gos)} objects, buttons: {btns[:8]}")
