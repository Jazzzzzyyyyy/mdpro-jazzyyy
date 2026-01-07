#!/bin/bash

###############################################################################
# MDPro3 macOS Build Script
#
# This script automates building MDPro3 for macOS by:
# 1. Fetching external dependencies from remote repositories
# 2. Creating symlinks in Assets/StreamingAssets
# 3. Building the project with Unity
#
# Usage:
#   ./build-macos.sh [OPTIONS]
#
# Options:
#   --force           Force re-download of all dependencies
#   --skip-deps       Skip dependency fetching
#   --skip-build      Skip Unity build (only fetch dependencies)
#   --unity-path PATH Path to Unity executable
#   --help            Show this help message
#
###############################################################################

set -e  # Exit on error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Default configuration
FORCE_DOWNLOAD=false
SKIP_DEPS=false
SKIP_BUILD=false
UNITY_PATH=""
EXTERNAL_DIR="$SCRIPT_DIR/external"
STREAMING_ASSETS_DIR="$SCRIPT_DIR/Assets/StreamingAssets"

# Dependency URLs and names
declare -A DEPENDENCIES=(
    ["platforms"]="https://code.moenext.com/sherry_chaos/mdpro3-assetbundles.git"
    ["hd-arts"]="https://code.mycard.moe/mycard/hd-arts.git"
    ["closeup"]="https://code.mycard.moe/mycard/ygopro2-closeup.git"
    ["sound"]="https://code.moenext.com/mycard/mdpro3-sound.git"
)

# Symlink mapping: local_name -> external_path
declare -A SYMLINK_MAPPING=(
    ["Platforms"]="platforms/Platforms"
    ["hd-arts"]="hd-arts/Picture"
    ["ygopro2-closeup"]="closeup/Picture"
    ["mdpro3-sound"]="sound"
)

###############################################################################
# Helper Functions
###############################################################################

log_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

log_success() {
    echo -e "${GREEN}✓${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

log_error() {
    echo -e "${RED}✗${NC} $1"
}

show_help() {
    cat << EOF
MDPro3 macOS Build Script

Usage: $0 [OPTIONS]

Options:
    --force           Force re-download of all dependencies
    --skip-deps       Skip dependency fetching
    --skip-build      Skip Unity build (only fetch dependencies)
    --unity-path PATH Path to Unity executable
    --help            Show this help message

Examples:
    # Normal build
    ./build-macos.sh

    # Force re-download dependencies
    ./build-macos.sh --force

    # Only fetch dependencies
    ./build-macos.sh --skip-build

    # Use specific Unity version
    ./build-macos.sh --unity-path /Applications/Unity/Hub/Editor/6000.0.10f1/Unity.app/Contents/MacOS/Unity

EOF
    exit 0
}

###############################################################################
# Parse Command Line Arguments
###############################################################################

parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --force)
                FORCE_DOWNLOAD=true
                shift
                ;;
            --skip-deps)
                SKIP_DEPS=true
                shift
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --unity-path)
                UNITY_PATH="$2"
                shift 2
                ;;
            --help)
                show_help
                ;;
            *)
                log_error "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
}

###############################################################################
# Find Unity Installation
###############################################################################

find_unity() {
    if [ -n "$UNITY_PATH" ] && [ -f "$UNITY_PATH" ]; then
        log_success "Using Unity at: $UNITY_PATH"
        return 0
    fi

    log_info "Searching for Unity installation..."

    # Common Unity paths on macOS
    local search_paths=(
        "/Applications/Unity/Hub/Editor/6000.0.10f1/Unity.app/Contents/MacOS/Unity"
        "/Applications/Unity/Hub/Editor/6000.0.24f1/Unity.app/Contents/MacOS/Unity"
        "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    )

    for path in "${search_paths[@]}"; do
        if [ -f "$path" ]; then
            UNITY_PATH="$path"
            log_success "Found Unity at: $UNITY_PATH"
            return 0
        fi
    done

    # Search all Unity installations
    local found_unity
    found_unity=$(find /Applications/Unity -name "Unity" -type f 2>/dev/null | grep "MacOS/Unity" | head -n 1)
    
    if [ -n "$found_unity" ]; then
        UNITY_PATH="$found_unity"
        log_success "Found Unity at: $UNITY_PATH"
        return 0
    fi

    log_error "Unity not found. Please install Unity or specify path with --unity-path"
    exit 1
}

###############################################################################
# Clone Repository with Retry Logic
###############################################################################

clone_with_retry() {
    local name=$1
    local url=$2
    local target=$3
    local max_attempts=3

    log_info "Fetching $name..."

    # Check if already exists and not forcing
    if [ -d "$target/.git" ] && [ "$FORCE_DOWNLOAD" = false ]; then
        log_success "$name already exists (use --force to re-download)"
        return 0
    fi

    # Remove existing directory if forcing or incomplete
    if [ -d "$target" ]; then
        log_warning "Removing existing $name directory..."
        rm -rf "$target"
    fi

    # Configure git for better network handling
    git config --global http.postBuffer 524288000
    git config --global http.lowSpeedLimit 1000
    git config --global http.lowSpeedTime 60
    git config --global http.version HTTP/1.1  # Avoid HTTP/2 stream issues

    for attempt in $(seq 1 $max_attempts); do
        log_info "Attempt $attempt/$max_attempts for $name..."

        # Use timeout to prevent hanging
        if timeout 600 git clone \
            --depth 1 \
            --single-branch \
            --no-tags \
            --progress \
            "$url" "$target" 2>&1; then
            
            # Verify clone integrity
            if [ -d "$target/.git" ] && [ -n "$(ls -A "$target")" ]; then
                log_success "$name cloned successfully"
                return 0
            else
                log_warning "$name clone appears incomplete"
                rm -rf "$target"
            fi
        fi

        if [ $attempt -lt $max_attempts ]; then
            local wait_time=$((attempt * 15))
            log_warning "Attempt $attempt failed, retrying in ${wait_time}s..."
            sleep $wait_time
        else
            log_error "Failed to clone $name after $max_attempts attempts"
            return 1
        fi
    done

    return 1
}

###############################################################################
# Fetch External Dependencies
###############################################################################

fetch_dependencies() {
    log_info "Fetching external dependencies..."

    # Create external directory
    mkdir -p "$EXTERNAL_DIR"
    cd "$EXTERNAL_DIR"

    # Clone each dependency
    local failed=()
    for dep_name in "${!DEPENDENCIES[@]}"; do
        local dep_url="${DEPENDENCIES[$dep_name]}"
        if ! clone_with_retry "$dep_name" "$dep_url" "$dep_name"; then
            failed+=("$dep_name")
        fi
    done

    cd "$SCRIPT_DIR"

    # Check for failures
    if [ ${#failed[@]} -gt 0 ]; then
        log_error "Failed to fetch dependencies: ${failed[*]}"
        exit 1
    fi

    log_success "All dependencies fetched successfully"
}

###############################################################################
# Create Symlinks
###############################################################################

create_symlinks() {
    log_info "Creating symlinks in Assets/StreamingAssets..."

    # Create StreamingAssets directory if it doesn't exist
    mkdir -p "$STREAMING_ASSETS_DIR"

    for link_name in "${!SYMLINK_MAPPING[@]}"; do
        local target_path="${SYMLINK_MAPPING[$link_name]}"
        local link_path="$STREAMING_ASSETS_DIR/$link_name"
        local full_target_path="$EXTERNAL_DIR/$target_path"

        # Remove existing symlink or directory
        if [ -L "$link_path" ]; then
            log_info "Removing existing symlink: $link_name"
            rm "$link_path"
        elif [ -d "$link_path" ]; then
            log_warning "Removing existing directory: $link_name"
            rm -rf "$link_path"
        fi

        # Verify target exists
        if [ ! -e "$full_target_path" ]; then
            log_error "Target does not exist: $full_target_path"
            continue
        fi

        # Create relative symlink
        local relative_target="../../external/$target_path"
        if ln -s "$relative_target" "$link_path"; then
            log_success "Created symlink: $link_name -> $relative_target"
        else
            log_error "Failed to create symlink: $link_name"
        fi
    done

    # List symlinks
    log_info "StreamingAssets contents:"
    ls -lah "$STREAMING_ASSETS_DIR" || true
}

###############################################################################
# Build with Unity
###############################################################################

build_with_unity() {
    log_info "Building macOS application with Unity..."

    # Find Unity if not specified
    if [ -z "$UNITY_PATH" ]; then
        find_unity
    fi

    # Create build directory
    local build_dir="$SCRIPT_DIR/build/StandaloneOSX"
    mkdir -p "$build_dir"

    # Build log file
    local log_file="$SCRIPT_DIR/build.log"

    log_info "Starting Unity build..."
    log_info "This may take several minutes..."

    # Run Unity build
    if "$UNITY_PATH" \
        -quit \
        -batchmode \
        -nographics \
        -projectPath "$SCRIPT_DIR" \
        -buildTarget StandaloneOSX \
        -buildOSXUniversalPlayer "$build_dir/MDPro3.app" \
        -logFile "$log_file" \
        -executeMethod BuildScript.BuildMac; then
        
        log_success "Unity build completed successfully"
        
        # Show last part of log
        echo ""
        log_info "Build log (last 50 lines):"
        tail -n 50 "$log_file"
        
        # Verify build
        if [ -d "$build_dir/MDPro3.app" ]; then
            log_success "Build artifact created: $build_dir/MDPro3.app"
            echo ""
            log_info "Build size:"
            du -sh "$build_dir/MDPro3.app"
        else
            log_error "Build artifact not found"
            exit 1
        fi
    else
        log_error "Unity build failed"
        echo ""
        log_info "Build log (last 100 lines):"
        tail -n 100 "$log_file"
        exit 1
    fi
}

###############################################################################
# Main Execution
###############################################################################

main() {
    echo ""
    log_info "MDPro3 macOS Build Script"
    echo ""

    # Parse arguments
    parse_arguments "$@"

    # Fetch dependencies unless skipped
    if [ "$SKIP_DEPS" = false ]; then
        fetch_dependencies
        create_symlinks
    else
        log_info "Skipping dependency fetch (--skip-deps specified)"
    fi

    # Build with Unity unless skipped
    if [ "$SKIP_BUILD" = false ]; then
        build_with_unity
    else
        log_info "Skipping Unity build (--skip-build specified)"
    fi

    echo ""
    log_success "Script completed successfully!"
    echo ""
}

# Run main function with all arguments
main "$@"
