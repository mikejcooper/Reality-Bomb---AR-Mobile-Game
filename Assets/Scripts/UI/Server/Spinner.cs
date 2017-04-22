using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spinner : MonoBehaviour {
    public GameObject IndicatorPrefab;
    private GameObject[] _playerIndicators;
    private CarController _randomCar;
    private float _v = 100;
    private int _rotations = 0;
    private float _target_rotation;
    private bool _slowdown = false;
    private bool _moving = true;
    private float _acc = 0.0f;
    private CarList _carList;

    public delegate void OnSpinnerFinished();
    public event OnSpinnerFinished OnSpinnerFinishedEvent = delegate { };
    
    public void init(CarList carList)
    {
        _carList = carList;
        int i = 0;
        int rand = Random.Range(0, carList._cars.Count);
        _randomCar = _carList._cars[rand];
        _playerIndicators = new GameObject[carList._cars.Count];
        foreach (var carController in _carList._cars)
        {
            PlayerDataManager.PlayerData playerData = ServerSceneManager.Instance.GetPlayerDataById(carController.ServerId);
            if ( i == rand )
            {
                _target_rotation = ((float)i / _carList._cars.Count) * 360 + 360;
            }
            _playerIndicators[i] = Instantiate(IndicatorPrefab);
            _playerIndicators[i].transform.SetParent(gameObject.transform);
            _playerIndicators[i].transform.localPosition = GetPosition(i);
            _playerIndicators[i].GetComponent<Image>().color = Color.HSVToRGB(playerData.colour / 360f, 1f, 0.8f); ;

            i++;
        }
    }

    private Vector3 GetPosition(int index)
    {
        float r = 100;
        float theta = 2 * Mathf.PI * ((float) index / _carList._cars.Count);
        return new Vector3(-r * Mathf.Sin(theta), r * Mathf.Cos(theta), 0);
    }

    void Update () {
        
        if (_moving)
        {
            float oldRotation = gameObject.transform.eulerAngles.z;
            _v = _v + _acc * Time.deltaTime;
            float s = _v * Time.deltaTime;
            gameObject.transform.Rotate(0, 0, s);

            if (gameObject.transform.eulerAngles.z - oldRotation < 0)
            {
                _rotations++;
                //Debug.Log(_rotations);
            }

            if (_rotations > 2 && !_slowdown)
            {
                _slowdown = true;
            }

            if (_slowdown && _acc == 0.0f)
            {
                float dist = _target_rotation - gameObject.transform.eulerAngles.z;
                _acc = -(_v * _v) / (2 * dist);
            }

            if (_v < 1)
            {
                _moving = false;
                OnSpinnerFinishedEvent(); //sets _assigning_bomb to false
                if (_carList.GetNumberOfBombsPresent() != 0)
                {
                    UnityEngine.Debug.LogError("There is already at least one car with a bomb.");
                }
                else
                {
                    _randomCar.setBombAllDevices(true);
                }
                    
                Invoke("Destroy", 2);
            }
        }        
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
