using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GridDataEditorWindow : EditorWindow
{

    private const string GRIDDATA_FOLDER_PATH = "Assets/_Scripts/Grid/GridData Objs/";

    private VisualElement _inspectorPanel;

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
            view.itemsSource.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( gridData.name )[ 0 ] ) ) );
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

        _inspectorPanel = new InspectorElement();
        splitView.Add( _inspectorPanel );

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

        var inspectorContainer = new VisualElement()
        {
            name = "Base-Custom-Element",
            style =
            {

                flexGrow = 1,
                flexDirection = FlexDirection.Row,
                alignItems = Align.FlexStart,
                justifyContent = Justify.SpaceAround

            }
        };

        if ( EditorWindow.HasOpenInstances<GridDataEditorWindow>() )
        {

            DrawEditor( inspectorContainer );

            serializedObject.ApplyModifiedProperties();

        }
        else
        {

            DrawOpenEditorButton( inspectorContainer );

        }

        return inspectorContainer;

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

    private void DrawGrid( VisualElement gridCellsContainer, SerializedProperty grid )
    {

        for ( int i = 0; i < grid.arraySize; i++ )
        {

            gridCellsContainer.Add( new PropertyField( grid.GetArrayElementAtIndex( i ) ) );

        }

    }

    private void DrawLegend( VisualElement gridDataLegendContainer, SerializedProperty cellSize )
    {

        var blackColorLegend = new VisualElement()
        {

            style =
                {

                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.SpaceBetween,
                    height = cellSize.intValue,
                    marginBottom = cellSize.intValue / 2

                }

        };
        gridDataLegendContainer.Add( blackColorLegend );

        var blackColorBlock = new VisualElement()
        {

            style =
                {

                    height = cellSize.intValue,
                    width = cellSize.intValue,
                    backgroundColor = Color.black

                }

        };
        blackColorLegend.Add( blackColorBlock );

        blackColorLegend.Add( new Label( "Empty" ) { style = { fontSize = 24, marginLeft = 10, alignSelf = Align.Center } } );

        var redColorLegend = new VisualElement()
        {

            style =
                {

                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.SpaceBetween,
                    height = cellSize.intValue

                }

        };
        gridDataLegendContainer.Add( redColorLegend );

        var redColorBlock = new VisualElement()
        {

            style =
                {

                    height = cellSize.intValue,
                    width = cellSize.intValue,
                    backgroundColor = Color.red

                }

        };
        redColorLegend.Add( redColorBlock );

        redColorLegend.Add( new Label( "Enemy Path" ) { style = { fontSize = 24, marginLeft = 10, alignSelf = Align.Center } } );

    }

    private void DrawInfoContainer( VisualElement gridInfoContainer, SerializedProperty rows, SerializedProperty columns, SerializedProperty cellSize )
    {

        var rowsInputField = new SliderInt( "Rows", 15, 25 )
        {
            value = rows.intValue,
            showInputField = true,
            style =
                {
                    height = 25,
                flexGrow = 1
                }
        };
        rowsInputField.BindProperty( rows );
        gridInfoContainer.Add( rowsInputField );

        var colsInputField = new SliderInt( "Columns", 15, 25 )
        {
            value = columns.intValue,
            showInputField = true,
            style =
                {
                    height = 25,
                    flexGrow = 1
                }
        };
        colsInputField.BindProperty( columns );
        gridInfoContainer.Add( colsInputField );

        var cellSizeInputField = new SliderInt( "Cell Size", 20, 60 )
        {
            value = 45,
            showInputField = true,
            style =
                {
                    height = 25,
                    flexGrow = 1
                }
        };
        cellSizeInputField.BindProperty( cellSize );
        gridInfoContainer.Add( cellSizeInputField );

    }

    private void DrawOpenEditorButton( VisualElement inspectorContainer )
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
        inspectorContainer.Add( button );

        button.clicked += () => GridDataEditorWindow.OpenGridDataEditor( (GridData)target );

        //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
        inspectorContainer.RegisterCallback<GeometryChangedEvent>( ( evt ) =>
        {

            if ( contentContainer == null )
            {

                var rootVisualContainer = FindParent<TemplateContainer>( inspectorContainer );

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

    private void DrawEditor( VisualElement inspectorContainer )
    {

        var grid = serializedObject.FindProperty( "_grid" );
        var rows = serializedObject.FindProperty( "_rows" );
        var columns = serializedObject.FindProperty( "_columns" );
        var cellSize = serializedObject.FindProperty( "_cellSize" );

        var gridDataLegendContainer = new VisualElement()
        {

            style =
                {

                    flexDirection = FlexDirection.Column,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.SpaceBetween,
                    width = Length.Auto(),
                    height = Length.Auto(),
                    marginTop = 10

                }

        };
        inspectorContainer.Add( gridDataLegendContainer );
        DrawLegend( gridDataLegendContainer, cellSize );

        var inspector = new VisualElement()
        {

            style =
            {

                flexGrow = 1,
                flexDirection = FlexDirection.Column,
                alignItems = Align.Center

            }

        };
        inspectorContainer.Add( inspector );

        var gridCellsContainer = new VisualElement()
        {

            style =
                {

                    width = cellSize.intValue * rows.intValue,
                    height = cellSize.intValue * columns.intValue,
                    flexDirection = FlexDirection.RowReverse,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.Center,
                    flexWrap = Wrap.Wrap

                }

        };
        inspector.Add( gridCellsContainer );
        DrawGrid( gridCellsContainer, grid );

        var gridInfoContainer = new VisualElement()
        {

            style =
                {

                    paddingTop = 15,
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1,
                    alignContent = Align.Stretch,
                    width = Length.Percent( 100 )

                }

        };
        inspector.Add( gridInfoContainer );
        DrawInfoContainer( gridInfoContainer, rows, columns, cellSize );

    }

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
    private SerializedProperty _statusProperty, _sizeProperty;
    private VisualElement _container, _parentContainer;

    public override VisualElement CreatePropertyGUI( SerializedProperty property )
    {

        _property = property;

        _sizeProperty = _property.FindPropertyRelative( "_size" );

        _statusProperty = _property.FindPropertyRelative( "_cellStatus" );

        _parentContainer = new VisualElement();
        _container = new VisualElement()
        {

            style =
            {

                width = Length.Percent( 100 ),
                height = Length.Percent( 100 )

            }

        };

        _parentContainer.Add( _container );

        _container.Add( DrawCell() );

        property.serializedObject.ApplyModifiedProperties();

        return _parentContainer;

    }

    private VisualElement DrawCell()
    {

        var element = new VisualElement()
        {

            style =
            {
                width = _sizeProperty.intValue,
                height = _sizeProperty.intValue,
                borderBottomWidth = 1,
                borderTopWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderBottomColor = Color.white,
                borderTopColor = Color.white,
                borderLeftColor = Color.white,
                borderRightColor = Color.white,
                backgroundColor = Color.black
            }

        };

        element.AddManipulator( new Clickable( ( evt ) =>
        {

            ToggleCellStatus( evt.target as VisualElement );

            UpdateCellState();

        } ) );

        return element;


    }

    private void ToggleCellStatus( VisualElement element )
    {

        Debug.Log( $"Clicked {element.name}" );

        if ( _statusProperty.intValue == (int)Cell.CellStatus.Empty )
        {

            _statusProperty.intValue = (int)Cell.CellStatus.Path;

            element.style.backgroundColor = Color.red;

        }
        else if ( _statusProperty.intValue == (int)Cell.CellStatus.Path )
        {

            _statusProperty.intValue = (int)Cell.CellStatus.Empty;

            element.style.backgroundColor = Color.black;

        }

        _statusProperty.serializedObject.ApplyModifiedProperties();

    }

    private void UpdateCellState()
    {

        _container.Clear();
        _container.Add( DrawCell() );

    }

}
