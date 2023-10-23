using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkedPlayers : NetworkBehaviour {

    public NetworkList<NetworkPlayerInfo> allNetPlayers;

    private int colorIndex = 0;
    private Color[] playerColors = new Color[] {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.grey,
        Color.cyan,
    };

    private void Awake() {
        allNetPlayers = new NetworkList<NetworkPlayerInfo>();
    } 

    void Start() {
        DontDestroyOnLoad(this.gameObject);
        if (IsServer) { 
            ServerStart();
        }
    }

    void ServerStart() {
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkPlayerInfo host = new NetworkPlayerInfo(NetworkManager.LocalClientId);
        host.ready = true;
        host.color = NextColor();
        host.playerName = "The Host";
        allNetPlayers.Add(host);
    }

    private void ServerOnClientConnected(ulong clientId) {
        NetworkPlayerInfo client = new NetworkPlayerInfo(clientId);
        client.ready = false;
        client.color = NextColor();
        client.playerName = $"Player {clientId}";
        allNetPlayers.Add(client);
    }

    private void ServerOnClientDisconnected(ulong clientId) {
        int idx = FindPlayerIndex(clientId);
        if (idx != -1) {
            allNetPlayers.RemoveAt(idx);
        }
    }

    private Color NextColor() {
        Color newColor = playerColors[colorIndex];
        colorIndex += 1;
        if (colorIndex > playerColors.Length - 1) {
            colorIndex = 0;
        }
        return newColor;
    }

    public int FindPlayerIndex(ulong clientId) {
        var idx = 0;
        var found = false;

        while (idx < allNetPlayers.Count && !found) {
            if (allNetPlayers[idx].clientId == clientId) {
                found = true;
            }
            else {
                idx += 1;
            }
        }
        if (!found) {
            idx = -1;
        }

        return idx;
    }

    public void UpdateReady(ulong clientId, bool ready) {
        int idx = FindPlayerIndex(clientId);
        if(idx == -1) return;
        
        NetworkPlayerInfo info = allNetPlayers[idx];
        info.ready = ready;
        allNetPlayers[idx] = info;
    }

    public string UpdatePlayerName(ulong clientId, string playerName) {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1) return "";
        
        NetworkPlayerInfo info = allNetPlayers[idx];
        if (playerName.Length < 10)
        {
            info.playerName = playerName;
            allNetPlayers[idx] = info;
        }
        return info.playerName.ToString();
    }

    public NetworkPlayerInfo GetMyInfo() {
        NetworkPlayerInfo toReturn = new NetworkPlayerInfo(ulong.MaxValue);
        int idx = FindPlayerIndex(NetworkManager.LocalClientId);
        if(idx != -1) {
            toReturn = allNetPlayers[idx];
        }
        return toReturn;
    }

    public bool AllPlayersReady() {
        bool theyAre = true;
        int idx = 0;
        while(theyAre && idx < allNetPlayers.Count) {
            theyAre = allNetPlayers[idx].ready;
            idx += 1;
        }
        return theyAre;
    }
}
