/* ============================
   GLOBAL APP CONFIG (ADD THIS AT TOP)
============================ */
var APP = APP || {};
APP.baseUrl = APP.baseUrl || '/';


/* ============================
   Google Sign-In Handler
============================ */
function handleGoogleSignIn(response) {

    $('#googleSignInContainer').hide();
    $('#googleSignInSpinner').show();

    $.ajax({
        url: APP.baseUrl + 'Auth/GoogleSignIn',
        type: 'POST',
        data: {
            idToken: response.credential
        },

        success: function (res) {

            if (!res) {
                showMessage('Server returned empty response', 'danger');
                return;
            }

            if (res.Errors && res.Errors.length > 0) {

                $('#googleSignInSpinner').hide();
                $('#googleSignInContainer').show();

                showMessage(res.Message || res.Errors[0], 'danger');
                return;
            }

            if (res.Data) {

                showMessage('Login successful! Redirecting...', 'success');

                setTimeout(function () {

                    if (res.Data.Role === 'ADMIN') {

                        window.location.href = APP.baseUrl + 'AdminDashboard';

                    } else {

                        window.location.href = APP.baseUrl;

                    }

                }, 500);
            }
        },

        error: function (xhr) {

            $('#googleSignInSpinner').hide();
            $('#googleSignInContainer').show();

            console.log(xhr.responseText);

            showMessage('Google login failed', 'danger');
        }
    });
}


/* ============================
   EMAIL LOGIN HANDLER (FIXED)
============================ */
$(document).ready(function () {

    $('#emailLoginForm').submit(function (e) {

        e.preventDefault();

        var email = $('#loginEmail').val().trim();
        var password = $('#loginPassword').val().trim();

        if (email === '' || password === '') {

            showMessage('Enter email and password', 'warning');
            return;
        }

        var $btn = $('#loginButton');

        $btn.find('.button-text').hide();
        $btn.find('.button-spinner').show();
        $btn.prop('disabled', true);


        $.ajax({

            url: '/Auth/UserLogin',   // HARD FIXED URL

            type: 'POST',

            dataType: 'json',

            data: {
                email: email,
                password: password
            },

            success: function (res) {

                console.log("LOGIN RESPONSE:", res);


                if (!res) {

                    showMessage('Invalid server response', 'danger');
                    resetButton($btn);
                    return;
                }


                if (res.Errors && res.Errors.length > 0) {

                    showMessage(res.Errors[0], 'danger');
                    resetButton($btn);
                    return;
                }


                if (res.Data) {

                    showMessage('Login success', 'success');

                    setTimeout(function () {

                        if (res.Data.Role === 'ADMIN') {

                            window.location.href = '/AdminDashboard';

                        } else {

                            window.location.href = '/';
                        }

                    }, 500);

                }
                else {

                    showMessage('Login failed', 'danger');
                    resetButton($btn);
                }
            },

            error: function (xhr) {

                console.log("LOGIN ERROR:", xhr.responseText);

                showMessage('Server error during login', 'danger');

                resetButton($btn);
            }

        });

    });

});


/* ============================
   EMAIL REGISTER HANDLER
============================ */
$(document).ready(function () {

    $('#emailRegisterForm').submit(function (e) {

        e.preventDefault();

        var name = $('#registerName').val();
        var email = $('#registerEmail').val();
        var password = $('#registerPassword').val();
        var confirmPassword = $('#registerConfirmPassword').val();

        var $btn = $('#registerButton');

        $btn.find('.button-text').hide();
        $btn.find('.button-spinner').show();
        $btn.prop('disabled', true);


        $.ajax({

            url: '/Auth/Register',

            type: 'POST',

            dataType: 'json',

            data: {

                name: name,
                email: email,
                password: password,
                confirmPassword: confirmPassword
            },

            success: function (res) {

                if (res.Errors && res.Errors.length > 0) {

                    showMessage(res.Errors[0], 'danger');

                    resetButton($btn);

                    return;
                }

                showMessage('Registration success', 'success');

                setTimeout(function () {

                    window.location.href = '/Auth/Login';

                }, 500);
            },

            error: function () {

                showMessage('Registration failed', 'danger');

                resetButton($btn);
            }

        });

    });

});


/* ============================
   LOGOUT
============================ */
function logout() {

    $.ajax({

        url: '/Auth/Logout',

        type: 'POST',

        success: function () {

            window.location.href = '/Auth/Login';

        },

        error: function () {

            window.location.href = '/Auth/Login';
        }

    });

}


/* ============================
   HELPER
============================ */

function showMessage(msg, type) {

    var $msg = $('#message');

    $msg.removeClass()
        .addClass('alert alert-' + type)
        .text(msg)
        .show();
}


function resetButton($btn) {

    $btn.find('.button-text').show();
    $btn.find('.button-spinner').hide();
    $btn.prop('disabled', false);
}
