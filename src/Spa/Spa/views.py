from flask import render_template, jsonify, request, abort, make_response
from Spa import app
import requests
from random import randint

API_URL = 'https://jwtsample.azurewebsites.net' #https://jwtsample.azurewebsites.net, http://localhost:28229

@app.route('/')
def home():
    return render_template('index.html')

@app.route('/auth/login', methods=['POST'])
def auth_local():
    try:
        r = requests.post(API_URL + "/api/auth/local", data=request.data, headers={'Content-type': 'application/json'})
        return r.text #todo: handle wrong user
    except:
        abort(503) #Service Unavailable

@app.route('/auth/google', methods=['POST'])
def auth_google():
    try:
        r = requests.post(API_URL + "/api/auth/google", data=request.data, headers={'Content-type': 'application/json'})
        return r.text
    except:
        abort(503) #Service Unavailable

@app.route('/auth/facebook', methods=['POST'])
def auth_facebook():
    try:
        r = requests.post(API_URL + "/api/auth/facebook", data=request.data, headers={'Content-type': 'application/json'})
        return r.text
    except:
        abort(503) #Service Unavailable

@app.route('/api/me')
def profile():
    try:
        r = requests.get(API_URL + "/api/profile", headers={'Authorization': request.headers['Authorization']})
        return r.text
    except:
        abort(503) #Service Unavailable

@app.route('/manifest')
def cache_manifest():
    manifest = """CACHE MANIFEST
#1.0

CACHE:
/
/static/bower_components/jquery/dist/jquery.min.js
/static/bower_components/bootstrap/dist/js/bootstrap.min.js
/static/bower_components/angular/angular.min.js
/static/bower_components/angular-ui-router/release/angular-ui-router.min.js
/static/bower_components/angular-animate/angular-animate.min.js
/static/bower_components/satellizer/satellizer.min.js
/static/bower_components/bootstrap/dist/fonts/glyphicons-halflings-regular.woff2
/static/bower_components/angular-messages/angular-messages.min.js
/static/bower_components/angular-resource/angular-resource.min.js
/static/bower_components/angular-strap/dist/angular-strap.min.js
/static/bower_components/angular-strap/dist/angular-strap.tpl.min.js
/static/app/app.js
/static/app/directives/passwordStrength.js
/static/app/directives/passwordMatch.js
/static/app/controllers/navbar.js
/static/app/controllers/login.js
/static/app/controllers/signup.js
/static/app/controllers/logout.js
/static/app/controllers/profile.js
/static/app/services/dbService.js
/static/app/services/syncService.js
/static/app/services/userService.js
/static/app/services/utilsService.js

/static/content/bootstrap-superhero.min.css
/static/content/site.css

/static/app/partials/home.html
/static/app/partials/login.html
/static/app/partials/profile.html
/static/app/partials/signup.html


NETWORK:
/api

    """
    
    #manifest = manifest + '# ' + str(randint(0,999))
    resp = make_response(manifest)
    resp.headers['Content-type'] = 'text/cache-manifest'
    return resp;