#!E:\Projects\JwtSample\src\Spa\env1\Scripts\python.exe
# EASY-INSTALL-ENTRY-SCRIPT: 'gunicorn==0.17.2','console_scripts','gunicorn_django'
__requires__ = 'gunicorn==0.17.2'
import sys
from pkg_resources import load_entry_point

if __name__ == '__main__':
    sys.exit(
        load_entry_point('gunicorn==0.17.2', 'console_scripts', 'gunicorn_django')()
    )
