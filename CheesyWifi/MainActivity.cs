using System;
using Android.App;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace CheesyWifi
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private NetworkCallback _callback;
        private TextView _statusText;
        private EditText _ssid;
        private EditText _passphrase;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var requestButton = FindViewById<Button>(Resource.Id.buttonRequest);
            var suggestButton = FindViewById<Button>(Resource.Id.buttonSuggest);
            _statusText = FindViewById<TextView>(Resource.Id.statusText);
            _ssid = FindViewById<EditText>(Resource.Id.ssid);
            _passphrase = FindViewById<EditText>(Resource.Id.passphrase);

            requestButton.Click += (s, e) => RequestNetwork();
            suggestButton.Click += (s, e) => SuggestNetwork();

            _callback = new NetworkCallback
            {
                NetworkAvailable = network =>
                {
                    // we are connected!
                    _statusText.Text = $"Request network available";
                },
                NetworkUnavailable = () =>
                {
                    _statusText.Text = $"Request network unavailable";
                }
            };
        }

        private void SuggestNetwork()
        {
            var suggestion = new WifiNetworkSuggestion.Builder()
                .SetSsid(_ssid.Text)
                .SetWpa2Passphrase(_passphrase.Text)
                .Build();

            var suggestions = new[] { suggestion };

            var wifiManager = GetSystemService(WifiService) as WifiManager;
            var status = wifiManager.AddNetworkSuggestions(suggestions);

            var statusText = status switch
            {
                NetworkStatus.SuggestionsSuccess => "Suggestion Success",
                NetworkStatus.SuggestionsErrorAddDuplicate => "Suggestion Duplicate Added",
                NetworkStatus.SuggestionsErrorAddExceedsMaxPerApp => "Suggestion Exceeds Max Per App"
            };

            _statusText.Text = statusText;
        }

        private bool _requested;
        private void RequestNetwork()
        {
            var specifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(_ssid.Text)
                .SetWpa2Passphrase(_passphrase.Text)
                .Build();

            var request = new NetworkRequest.Builder()
                .AddTransportType(TransportType.Wifi)
                .SetNetworkSpecifier(specifier)
                .Build();

            var connectivityManager = GetSystemService(ConnectivityService) as ConnectivityManager;

            if (_requested)
            {
                connectivityManager.UnregisterNetworkCallback(_callback);
            }

            connectivityManager.RequestNetwork(request, _callback);
            _requested = true;
        }

        private class NetworkCallback : ConnectivityManager.NetworkCallback
        {
            public Action<Network> NetworkAvailable { get; set; }
            public Action NetworkUnavailable { get; set; }

            public override void OnAvailable(Network network)
            {
                base.OnAvailable(network);
                NetworkAvailable?.Invoke(network);
            }

            public override void OnUnavailable()
            {
                base.OnUnavailable();
                NetworkUnavailable?.Invoke();
            }
        }
    }
}

