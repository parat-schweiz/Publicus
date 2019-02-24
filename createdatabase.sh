#!/bin/bash

su -c 'createuser --username=postgres --no-superuser --pwprompt publicus' postgres
su -c 'createdb --username=postgres --owner=publicus --encoding=UNICODE publicus' postgres

