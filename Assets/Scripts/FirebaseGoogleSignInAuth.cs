using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseGoogleSignInAuth : MonoBehaviour
{
    private string _webClientId = "982715551252-nndsdi9f9a2qrr05k7ance020o5gh26m.apps.googleusercontent.com";

    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _config;

    [SerializeField] private Button _signInButton;

    private void Awake()
    {
        _auth = FirebaseAuth.DefaultInstance;

        _config = new GoogleSignInConfiguration
        {
            WebClientId = _webClientId,
            UseGameSignIn = false, //Google Play Games 서비스 사용 여부
            RequestIdToken = true,
            RequestEmail = true
        };

        GoogleSignIn.Configuration = _config;
    }

    private void OnEnable()
    {
        _signInButton?.onClick.AddListener(SignIn);
    }

    private void OnDisable()
    {
        _signInButton?.onClick.RemoveListener(SignIn);
    }

    private void Start()
    {
        _signInButton?.gameObject.SetActive(true);
    }

    public void SignIn()
    {
#if UNITY_ANDROID || UNITY_IOS
        SignInAsync().Forget();
#else
        //AdManager.Instance.ShowLaunchInterstitialOnce();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(authTask =>
                {
                    if (authTask.IsCompleted && authTask.IsFaulted == false && authTask.IsCanceled == false)
                    {
                        GameManager.Instance.firebaseReady = true;
                    }
                    else
                    {
                        Debug.LogError($"익명 로그인 실패, {authTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"파이어베이스 에러, {task.Result}");
            }
        });

        _signInButton?.gameObject.SetActive(false);

#endif
    }

    public async UniTaskVoid SignInAsync()
    {
        try
        {
            //구글 계정 선택 UI 호출
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
            {
                Debug.LogError("[Auth] Google Sign-In 실패 : IdToken 없음");
                ObjectPoolManager.Instance.uiPool.GetMessage().LogError("[Auth] Google Sign-In 실패 : IdToken 없음");
                return;
            }

            //파이어베이스 자격 증명 생성
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);


            //파이어베이스 로그인
            FirebaseUser firebaseUser = await _auth.SignInWithCredentialAsync(credential);

            GameManager.Instance.firebaseReady = true;

            _signInButton?.gameObject.SetActive(false);
        }
        catch (GoogleSignIn.SignInException e)
        {
            Debug.LogError($"[Auth] 로그인 예외 : {e.Message}, {e.Status}"); 
            ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[Auth] 로그인 예외 : {e.Message}, {e.Status}");
        }
    }

    public void SignOut()
    {
        UIManager.Instance.PopupOpen<UIConfirmPopup>().Init(() =>
        {
            try
            {
                GoogleSignIn.DefaultInstance.SignOut();
                _auth.SignOut();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Auth] 로그아웃 예외 : {e}");
                ObjectPoolManager.Instance.uiPool.GetMessage().LogError($"[Auth] 로그아웃 예외 : {e.Message}");
            }
            finally
            {
                Application.Quit();
            }
        }
        , "UI_EnsureSignOut");
    }
}
