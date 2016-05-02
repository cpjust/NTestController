#!/bin/bash

seqdiag > /dev/null 2>&1

# Check if seqdiag is installed.
if [[ "$?" -ne "0" ]]; then
    echo "seqdiag not found.  To install seqdiag run:  sudo apt-get install python-seqdiag"
fi

set -e

seqdiag Plugin-Sequence.diag -Tsvg

