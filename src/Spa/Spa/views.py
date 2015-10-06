from flask import render_template, jsonify, request, abort
from Spa import app
import requests

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