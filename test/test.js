var assert = require('assert'),
    request = require('supertest'),
    cheerio = require('cheerio'),
    app = 'http://localhost:8080/';

var username = "admin"; //Test user.
var password = "admin"; //Test user.
var location = ""; //X-Referer

describe('SignIn app', function () {
    it('Should respond with html when Accept="text/html"', function (done) {
        request(app)
        .get('signin/signinuser')
        .set('Accept', 'text/html')
        .expect('Content-Type', /html/)
        .expect(200, done);
    });

    it('Should deny unauthorised request to protected content when Accept="text/plain"', function (done) {
        request(app)
        .get('UserAdmin/admin/users')
        .set('Accept', 'text/plain')
        .expect('Content-Type', 'text/plain')
        .expect(function (res) {
            var data = JSON.parse(res.text);
            var workspace = null;

            for (var i = 0; i < data.workspaces.length; i++) {
                if (data.workspaces[i].AppName == "UserAdmin") {
                    workspace = data.workspaces[i];
                    break;
                }
            }

            assert(workspace, "There should be UserAdmin workspace");
            assert(workspace.UserAdmin.RedirectUrl$, "RedirectUrl$ should be set because of unauthorised request");
        })
        .expect(200, done);
    });

    it('Should sign in user when Accept="text/plain"', function (done) {
        request(app)
        .get('signin/signin/' + username + '/' + password)
        .set('Accept', 'text/plain')
        .expect('Content-Type', 'text/plain')
        .expect(function (res) {
            var data = JSON.parse(res.text.replace(/,,/gi, ","));
            var user = data.user.SignIn;
            var cookies = res.header['set-cookie'];

            for (var i = 0; i < cookies.length; i++) {
                if (/^Location/gi.test(cookies[i])) {
                    location = cookies[i].replace(/^Location=/gi, '').replace(/;.*$/gi, '').replace(/[%]2F/gi, "/");
                    break;
                }
            }

            assert(location, "Location cookie should be set");
            assert(user.IsSignedIn, "User should be signed in");
            assert(user.FullName, "User name should be set");
        })
        .expect(200, done);
    });

    it('Should accept authorised request to protected content when Accept="text/plain"', function (done) {
        request(app)
        .get('UserAdmin/admin/users')
        .set('Accept', 'text/plain')
        .set('X-Referer', location)
        .expect('Content-Type', 'text/plain')
        .expect(function (res) {
            var data = JSON.parse(res.text.replace(/,,/gi, ","));
            var workspace = null;

            for (var i = 0; i < data.workspaces.length; i++) {
                if (data.workspaces[i].AppName == "UserAdmin") {
                    workspace = data.workspaces[i];
                    break;
                }
            }

            assert(workspace, "There should be UserAdmin workspace");
            assert(!workspace.UserAdmin.RedirectUrl$, "RedirectUrl$ should be blank for authorised request");
            assert(workspace.UserAdmin.Items, "There should be some items");
        })
        .expect(200, done);
    });

    it('Should sign out when Accept="text/plain"', function (done) {
        request(app)
        .get('signin/signout')
        .set('Accept', 'text/plain')
        .expect('Content-Type', 'text/plain')
        .expect(function (res) {
            var data = JSON.parse(res.text.replace(/,,/gi, ","));
            var user = data.user.SignIn;

            assert(!user.IsSignedIn, "User should be signed out");
            assert(!user.FullName, "User name should be empty");
        })
        .expect(200, done);
    });
});
