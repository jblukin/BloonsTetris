using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class GridDataEditorWindow : EditorWindow
{

    private const string GRIDDATA_FOLDER_PATH = "Assets/_Scripts/Grid/GridData Objs/";

    private VisualElement _gridPanel, _inspectorPanel;

    private static GridData _gridData;

    private void CreateGUI()
    {

        var GridDataGUIDs = AssetDatabase.FindAssets( "t:GridData" );

        List<GridData> allGridDataObjs = new();

        foreach ( var guid in GridDataGUIDs )
            allGridDataObjs.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( guid ) ) );

        if ( _gridData != null && allGridDataObjs.Count > 0 )
            _gridData = allGridDataObjs.FirstOrDefault();

        var splitView = new TwoPaneSplitView( 0, 240f, TwoPaneSplitViewOrientation.Horizontal );
        rootVisualElement.Add( splitView );

        var leftView = new ListView
        {
            makeItem = () => new Label(),
            bindItem = ( item, idx ) => { ( item as Label ).text = allGridDataObjs[ idx ].name; },
            itemsSource = allGridDataObjs,
            selectionType = SelectionType.Single,
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
            fixedItemHeight = 25,
            showAddRemoveFooter = true,
            style =
            {

                flexGrow = 1,
                flexShrink = 1

            }
        };
        splitView.Add( leftView );

        leftView.selectionChanged += OnGridDataSelectionChange;
        leftView.onAdd = view =>
        {
            var gridData = CreateNewGridData();
            if ( gridData == null )
                return;
            view.itemsSource.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( gridData.name )[0] ) ) );
            view.RefreshItems();
            view.SetSelection( view.itemsSource.Count );
            view.ScrollToItem( view.itemsSource.Count );

        };

        leftView.onRemove = view =>
        {

            if ( view.itemsSource.Count == 0 )
                return;
            GridData asset = view.selectedItem as GridData;
            view.itemsSource.RemoveAt( view.selectedIndex );
            view.RefreshItems();
            if ( view.selectedIndex == 0 )
                view.selectedIndex++;
            view.SetSelection( --view.selectedIndex );
            view.ScrollToItem( view.selectedIndex );
            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( asset ) );

        };

        var rightView = new TwoPaneSplitView( 1, 200f, TwoPaneSplitViewOrientation.Vertical );

        _gridPanel = new VisualElement();
        rightView.Add( _gridPanel );

        _inspectorPanel = new InspectorElement();
        rightView.Add( _inspectorPanel );

        splitView.Add( rightView );

        leftView.schedule.Execute( () => leftView.SetSelection( leftView.itemsSource.IndexOf( _gridData ) ) );

    }

    private GridData CreateNewGridData()
    {

        EditorInputPopupWindow.PromptInput( "Create New GridData Asset", "Please define GridData properties:", position, 
            out string assetName, out int rows, out int cols, out int cellSize );

        if ( string.IsNullOrEmpty( assetName ) )
            return null;

        var gridData = CreateInstance<GridData>();

        gridData.name = assetName;
        gridData.CreateGrid( rows, cols, cellSize );

        AssetDatabase.CreateAsset( gridData, GRIDDATA_FOLDER_PATH + assetName + ".asset" );

        return gridData;

    }

    private void OnGridDataSelectionChange( IEnumerable<object> selectedItems )
    {

        _gridPanel.Clear();
        _inspectorPanel.Clear();

        var enumerator = selectedItems.GetEnumerator();

        if ( enumerator.MoveNext() )
        {

            var selected = enumerator.Current as GridData;

            if ( enumerator.Current != null )
            {

                var inspector = new InspectorElement( selected );
                _inspectorPanel.Add( inspector );

            }

        }

    }

    [OnOpenAsset()]
    public static bool OpenGridDataEditor( int instanceID )
    {

        GridData gridData = EditorUtility.InstanceIDToObject( instanceID ) as GridData;

        if ( gridData != null )
        {

            OpenGridDataEditor( gridData );
            return true;

        }

        return false;

    }

    public void OnDestroy()
    {

       var gridDataEditor = FindFirstObjectByType( typeof( GridDataEditor ) ) as GridDataEditor;

        if ( gridDataEditor != null )
        {

            gridDataEditor.Repaint();

        }

    }

    public static void OpenGridDataEditor( GridData gridData )
    {

        GridDataEditorWindow window = GetWindow<GridDataEditorWindow>( "Grid Data Editor" );

        _gridData = gridData;

        window.minSize = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );
        window.maxSize = new Vector2( Screen.currentResolution.width / 1.5f, Screen.currentResolution.height / 1.5f );

        window.Center();

    }

    [MenuItem( "Tools/Grid Data Editor" )]
    public static void OpenGridDataEditor()
    {

        GridDataEditorWindow window = GetWindow<GridDataEditorWindow>( "Grid Data Editor" );
        
        _gridData = null;

        window.minSize = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );
        window.maxSize = new Vector2( Screen.currentResolution.width / 1.5f, Screen.currentResolution.height / 1.5f );

        window.Center();

    }

}

public class EditorInputPopupWindow : EditorWindow
{

    public static void PromptInput( string title, string desc, Rect parentRect, out string assetName, out int rows, out int cols, out int cellSize, string placeholderInputText = "Name your new GridData", string confirmButtonText = "Confirm", string cancelButtonText = "Cancel" )
    {

        string userInput = string.Empty;
        int inputRows = 0, inputCols = 0, inputCellSize = 0;

        EditorInputPopupWindow window = GetWindow<EditorInputPopupWindow>( title );

        window.rootVisualElement.Add( new Label( desc ) { style = { fontSize = 24 } } );

        var gridInfoContainer = new VisualElement();
        gridInfoContainer.style.flexGrow = 1;
        gridInfoContainer.style.alignContent = Align.Stretch;
        gridInfoContainer.style.alignSelf = Align.Stretch;
        window.rootVisualElement.Add( gridInfoContainer );

        var rowsInputField = new SliderInt( "Rows", 1, 25 )
        {
            value = 15,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( rowsInputField );

        var colsInputField = new SliderInt( "Columns", 1, 25 )
        {
            value = 15,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( colsInputField );

        var cellSizeInputField = new SliderInt( "Cell Size", 40, 60 )
        {
            value = 45,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( cellSizeInputField );

        var nameInputField = new TextField( "Asset Name" )
        {
            textEdition =
            {
                placeholder = placeholderInputText,
                hidePlaceholderOnFocus = true
            },
            style =
            {
                height = 25
            }

        };
        gridInfoContainer.Add( nameInputField );

        var buttonContainer = new VisualElement();
        gridInfoContainer.Add( buttonContainer );
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.alignSelf = Align.Stretch;
        buttonContainer.style.justifyContent = Justify.SpaceAround;

        var confirmButton = new Button( () =>
        {
            userInput = nameInputField.value;
            inputRows = rowsInputField.value;
            inputCols = colsInputField.value;
            inputCellSize = cellSizeInputField.value;
            window.Close();
        } )
        {
            text = confirmButtonText,
            style =
            {
                height = 40,
                width = StyleKeyword.Auto,
                flexGrow = 1,
                flexDirection = FlexDirection.Column
            }
        };
        buttonContainer.Add( confirmButton );

        var cancelButton = new Button( () =>
        {
            userInput = string.Empty;
            inputRows = 0;
            inputCols = 0;
            inputCellSize = 0;
            window.Close();
        } )
        {
            text = cancelButtonText,
            style =
            {
                height = 40,
                width = StyleKeyword.Auto,
                flexGrow = 1,
                flexDirection = FlexDirection.Column
            }
        };
        buttonContainer.Add( cancelButton );

        window.rootVisualElement.RegisterCallback<KeyUpEvent>( evt =>
        {

            if ( evt.keyCode == KeyCode.Escape )
            {

                userInput = string.Empty;
                inputRows = 0;
                inputCols = 0;
                inputCellSize = 0;

                window.Close();

            }

            if ( evt.keyCode == KeyCode.Return )
            {

                userInput = nameInputField.value;
                inputRows = rowsInputField.value;
                inputCols = colsInputField.value;
                inputCellSize = cellSizeInputField.value;

                window.Close();

            }

        } );

        window.position = new Rect( 0, 0, parentRect.width * 0.20f, parentRect.height * 0.125f );

        window.minSize = new( parentRect.width * 0.4f, parentRect.height * 0.25f );
        window.maxSize = window.minSize;

        window.Center();

        window.ShowModalUtility();

        assetName = userInput;
        rows = inputRows;
        cols = inputCols;
        cellSize = inputCellSize;

    }

}

[CustomEditor( typeof( GridData ) )]
public class GridDataEditor : Editor
{

    private VisualElement contentContainer;

    public override VisualElement CreateInspectorGUI()
    {

        var inspector = new VisualElement()
        {
            name = "Base-Custom-Element",
            style =
            {

                flexGrow = 1

            }
        };

        if ( EditorWindow.HasOpenInstances<GridDataEditorWindow>() )
        {

            var rows = serializedObject.FindProperty( "_rows" );
            var columns = serializedObject.FindProperty( "_columns" );
            var cellSize = serializedObject.FindProperty( "_cellSize" );

            var gridInfoContainer = new VisualElement();
            gridInfoContainer.style.flexGrow = 1;
            gridInfoContainer.style.alignContent = Align.Stretch;
            gridInfoContainer.style.alignSelf = Align.Stretch;
            inspector.Add( gridInfoContainer );

            var rowsInputField = new SliderInt( "Rows", 1, 25 )
            {
                value = rows.intValue,
                showInputField = true,
                style =
                {
                height = 25,
                alignContent = Align.Stretch
                }
            };
            rowsInputField.BindProperty( rows );
            gridInfoContainer.Add( rowsInputField );

            var colsInputField = new SliderInt( "Columns", 1, 25 )
            {
                value = columns.intValue,
                showInputField = true,
                style =
                {
                height = 25,
                alignContent = Align.Stretch
                }
            };
            colsInputField.BindProperty( columns );
            gridInfoContainer.Add( colsInputField );

            var cellSizeInputField = new SliderInt( "Cell Size", 40, 60 )
            {
                value = 45,
                showInputField = true,
                style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
            };
            cellSizeInputField.BindProperty( cellSize );
            gridInfoContainer.Add( cellSizeInputField );

            serializedObject.ApplyModifiedProperties();

        }
        else
        {

            var button = new Button
            {
                text = "Open Editor",
                style =
                {
                    fontSize = 48,
                    flexGrow = 1,
                    flexShrink = 1,
                    flexBasis = StyleKeyword.Auto
                }
            };
            inspector.Add( button );

            button.clicked += () => GridDataEditorWindow.OpenGridDataEditor( (GridData)target );

            //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
            inspector.RegisterCallback<GeometryChangedEvent>( ( evt ) =>
            {

                if ( contentContainer == null )
                {

                    var rootVisualContainer = FindParent<TemplateContainer>( inspector );

                    if ( rootVisualContainer != null )
                        contentContainer = rootVisualContainer.Query<VisualElement>().Where( container => container.name == "unity-content-container" ).AtIndex( 1 );

                }

                if ( contentContainer != null )
                {

                    contentContainer.parent.style.flexGrow = 1;
                    contentContainer.style.flexGrow = 1;
                    contentContainer.style.height = StyleKeyword.Auto;
                    var editorList = contentContainer.Q<VisualElement>( className: "unity-inspector-editors-list" );
                    editorList.style.flexGrow = 1;
                    editorList.Children().ElementAt( 1 ).style.flexGrow = 1;
                    SetBoxModelValuesToSame( contentContainer.Q<InspectorElement>(), 10, 0, 5 );
                    button.StretchToParentSize();


                }

            } );
            //End Sourced Code

        }

        return inspector;

    }

    //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
    private static T FindParent<T>( VisualElement element, string name = null ) where T : VisualElement
    {
        VisualElement parent = element;

        do
        {

            parent = parent.parent;

            if ( parent != null && parent.GetType() == typeof( T ) )
            {

                if ( !string.IsNullOrEmpty( name ) && parent.name != name )
                    continue;

                return (T)parent;

            }

        } while ( parent != null );

        return null;

    }
    //End Sourced Code

    private void SetBoxModelValuesToSame( VisualElement element, int margin = 0, int border = 0, int padding = 0 )
    {

        element.style.paddingBottom = padding;
        element.style.paddingLeft = padding;
        element.style.paddingRight = padding;
        element.style.paddingTop = padding;
        element.style.marginBottom = margin;
        element.style.marginLeft = margin;
        element.style.marginRight = margin;
        element.style.marginTop = margin;
        element.style.borderBottomWidth = border;
        element.style.borderLeftWidth = border;
        element.style.borderRightWidth = border;
        element.style.borderTopWidth = border;

    }

}

[CustomPropertyDrawer( typeof( Cell ) )]
public class CellDrawer : PropertyDrawer
{

    private SerializedProperty _property;
    private SerializedProperty _statusProperty;
    private VisualElement _container, _parentContainer;

    public override VisualElement CreatePropertyGUI( SerializedProperty property )
    {

        _property = property;
        _parentContainer = new VisualElement();
        _container = new VisualElement();

        _parentContainer.Add( _container );

        _container.Add( DrawCell() );

        property.serializedObject.ApplyModifiedProperties();

        return _parentContainer;

    }

    private VisualElement DrawCell()
    {

        var sizeProperty = _property.FindPropertyRelative( "Size" );

        var element = new VisualElement()
        {

            style =
            {
                width = sizeProperty.floatValue * 2,
                height = sizeProperty.floatValue * 2,
                flexGrow = 1,
                flexShrink = 1,
                flexBasis = StyleKeyword.Auto
            }

        };

        element.AddManipulator( new Clickable( ( evt ) =>
        {

            CycleCellStatus( evt.target as VisualElement );

            UpdateCellState();

        } ) );

        _statusProperty = _property.FindPropertyRelative( "Status" );

        if ( _statusProperty.intValue == (int)Cell.CellStatus.Empty )
        {




        }
        else if ( _statusProperty.intValue == (int)Cell.CellStatus.Occupied )
        {




        }
        else
        {



        }

        return element;


    }

    private void CycleCellStatus( VisualElement element )
    {

        if ( _statusProperty.enumValueIndex + 1 > _statusProperty.enumNames.Length )
        {

            _statusProperty.intValue = 0;

        }
        else
            _statusProperty.intValue++;

        _statusProperty.serializedObject.ApplyModifiedProperties();

    }

    private void UpdateCellState()
    {

        _container.Clear();
        _container.Add( DrawCell() );

    }

}
