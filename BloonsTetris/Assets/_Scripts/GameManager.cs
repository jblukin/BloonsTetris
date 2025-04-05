using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    [SerializeField] private MapGenerator _mapGenerator;

    [SerializeField] private GameObject _tetriminoPrefab;

    private InputAction _mouseHover, _mouseDrag;

    private List<GameObject> _allTetriminos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _allTetriminos = new List<GameObject>();

        if(_mapGenerator == null) _mapGenerator = GetComponent<MapGenerator>();

        _mouseHover = InputSystem.actions.FindAction( "MouseHover" );

        _mouseDrag = InputSystem.actions.FindAction( "MouseDrag" );

        _allTetriminos.Add( Instantiate( _tetriminoPrefab ) );

        _mapGenerator.GenerateBasicGrid( _mapGenerator.Rows, _mapGenerator.Cols );

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( _mouseHover.ReadValue<Vector2>() );

        _mapGenerator.HighlightCurrentGridCell( mouseWorldPos );

        //Maybe just implement Events in Tetrimino?
        Ray mouseRay = Camera.main.ScreenPointToRay( mouseWorldPos );

        if(Physics.Raycast(mouseRay, out RaycastHit hit))
        {

            if(hit.transform.gameObject.tag == "Tetrimino")
            {

                

            }

        }

    }
}
